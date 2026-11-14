using System.ComponentModel;
using System.Diagnostics;
using inpsNuGet;
using inpsIDE.Controls;
using inpsIDE.Languages;
using inpsIDE.Models;
using inpsIDE.Services;

namespace inpsIDE.Forms;

/// <summary>The IDE window: project tree on the left, tabbed code editors on the right, interactive console at the bottom.</summary>
public sealed class MainForm : Form
{
    sealed record FsNode(string Path, bool IsDirectory);

    const string PlaceholderName = "__loading__";
    const int FolderIcon = 0, FileIcon = 1;
    const int MaxFileBytes = 5_000_000;

    readonly IdeProject _project;
    readonly StartForm _startForm;
    readonly bool _serverCapable;

    readonly TreeView _tree = new()
    {
        Dock = DockStyle.Fill,
        HideSelection = false,
        ShowLines = false,
        FullRowSelect = true,
        BorderStyle = BorderStyle.None,
        ItemHeight = 22
    };
    readonly TabControl _tabs = new() { Dock = DockStyle.Fill };
    readonly ConsolePanel _console = new() { Dock = DockStyle.Fill };
    readonly ToolStripStatusLabel _statusMessage = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    readonly ToolStripStatusLabel _statusPosition = new("");
    readonly ProcessRunner _runner = new();
    readonly ProcessRunner _server = new();

    ToolStripMenuItem _runItem = null!, _serverStartItem = null!, _serverStopItem = null!;
    bool _exitRequested;
    string? _serverUrl;

    public MainForm(IdeProject project, StartForm startForm)
    {
        _project = project;
        _startForm = startForm;
        _serverCapable = project.Language?.SupportsServer(project.Template) == true;

        Text = $"{project.Name} - inpsIDE";
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1280, 780);   // size used when the user restores the window
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;

        BuildUi();
        LoadTree();
        OpenInitialFile();
        WireRunners();
        UpdateRunState();
        SetStatus(DescribeRuntime());
    }

    IEnumerable<EditorTab> Tabs => _tabs.TabPages.OfType<EditorTab>();

    string DescribeRuntime()
    {
        if (_project.Language is not { } language) return "Ready (this project has no language runtime).";
        return _project.RuntimeExecutable is { } exe
            ? $"{language.DisplayName} runtime: {exe}"
            : $"No {language.DisplayName} runtime found in this project (running is disabled).";
    }

    // ------------------------------------------------------------------ UI

    void BuildUi()
    {
        MenuStrip menu = BuildMenu();

        var status = new StatusStrip();
        status.Items.Add(_statusMessage);
        status.Items.Add(_statusPosition);

        _tree.ImageList = BuildIcons();
        _tree.BeforeExpand += (_, e) =>
        {
            if (e.Node is { } node && node.Nodes.Count == 1 && node.Nodes[0].Name == PlaceholderName)
                PopulateDirectory(node);
        };
        _tree.NodeMouseDoubleClick += (_, e) =>
        {
            if (e.Node.Tag is FsNode { IsDirectory: false } file) OpenFile(file.Path);
        };
        _tree.NodeMouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Right) _tree.SelectedNode = e.Node;
        };
        _tree.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Delete) DeleteSelected();
            else if (e.KeyCode == Keys.Enter && SelectedFs is { IsDirectory: false } file) OpenFile(file.Path);
        };
        _tree.ContextMenuStrip = BuildTreeMenu();

        _tabs.SelectedIndexChanged += (_, _) => UpdateCaretStatus();
        _tabs.MouseUp += (_, e) =>
        {
            if (e.Button != MouseButtons.Middle) return;
            for (int i = 0; i < _tabs.TabCount; i++)
            {
                if (_tabs.GetTabRect(i).Contains(e.Location) && _tabs.TabPages[i] is EditorTab tab)
                {
                    CloseTab(tab);
                    break;
                }
            }
        };

        var right = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, FixedPanel = FixedPanel.Panel2 };
        right.Panel1.Controls.Add(_tabs);
        right.Panel2.Controls.Add(_console);

        var main = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1 };
        main.Panel1.Controls.Add(_tree);
        main.Panel2.Controls.Add(right);

        Controls.Add(main);
        Controls.Add(menu);
        Controls.Add(status);
        MainMenuStrip = menu;
        main.BringToFront();

        // Shown (not Load): the window is maximized by then, so the splitters get the real height.
        Shown += (_, _) =>
        {
            TrySplit(main, 260);
            TrySplit(right, right.Height - 240);
        };
    }

    static void TrySplit(SplitContainer container, int distance)
    {
        try { container.SplitterDistance = Math.Max(distance, container.Panel1MinSize); }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException) { }
    }

    MenuStrip BuildMenu()
    {
        _runItem = Item("&Run Current File", Keys.F5, RunCurrentFile);
        _serverStartItem = Item("&Start Web Server", Keys.Control | Keys.F5, StartServer);
        _serverStopItem = Item("S&top Web Server", Keys.Shift | Keys.F5, StopServer);

        var file = new ToolStripMenuItem("&File");
        file.DropDownItems.AddRange(new ToolStripItem[]
        {
            Item("&New File...", Keys.Control | Keys.N, () => NewItem(isFolder: false)),
            Item("&Save", Keys.Control | Keys.S, SaveCurrent),
            Item("Save &All", Keys.Control | Keys.Shift | Keys.S, SaveAll),
            Item("&Close Tab", Keys.Control | Keys.W, CloseCurrentTab),
            new ToolStripSeparator(),
            Item("Close &Project", Keys.None, () => Close()),
            Item("E&xit", Keys.None, ExitApplication)
        });

        var run = new ToolStripMenuItem("&Run");
        run.DropDownItems.AddRange(new ToolStripItem[] { _runItem, new ToolStripSeparator(), _serverStartItem, _serverStopItem });

        var help = new ToolStripMenuItem("&Help");
        //help.DropDownItems.Add(Item("&About inpsIDE", Keys.None, () =>
        //    MessageBox.Show(this,
        //        $"inpsIDE\nA lightweight IDE for {LanguageRegistry.DescribeAll()}, built with C# WinForms on .NET 10.",
        //        "About", MessageBoxButtons.OK, MessageBoxIcon.Information)));
        help.DropDownItems.Add(Item("&About inpsIDE", Keys.None, () =>
            MessageBox.Show(this,
                $"inpsIDE\nThe official IDE made by inpsStudio, built with C# WinForms on .NET 10.",
                "About", MessageBoxButtons.OK, MessageBoxIcon.Information)));

        var menu = new MenuStrip();
        menu.Items.AddRange(new ToolStripItem[] { file, run, help });
        return menu;
    }

    static ToolStripMenuItem Item(string text, Keys shortcut, Action onClick)
    {
        var item = new ToolStripMenuItem(text);
        if (shortcut != Keys.None) item.ShortcutKeys = shortcut;
        item.Click += (_, _) => onClick();
        return item;
    }

    ContextMenuStrip BuildTreeMenu()
    {
        var menu = new ContextMenuStrip();
        var deleteItem = Item("&Delete", Keys.None, DeleteSelected);
        menu.Items.Add(Item("New &File...", Keys.None, () => NewItem(isFolder: false)));
        menu.Items.Add(Item("New F&older...", Keys.None, () => NewItem(isFolder: true)));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(deleteItem);
        menu.Items.Add(Item("&Refresh", Keys.None, RefreshSelected));
        menu.Opening += (_, _) => deleteItem.Enabled = _tree.SelectedNode?.Parent is not null; // never the root
        return menu;
    }

    static ImageList BuildIcons()
    {
        var list = new ImageList { ColorDepth = ColorDepth.Depth32Bit, ImageSize = new Size(16, 16) };

        var folder = new Bitmap(16, 16);
        using (Graphics g = Graphics.FromImage(folder))
        using (var tabBrush = new SolidBrush(Color.FromArgb(214, 160, 40)))
        using (var bodyBrush = new SolidBrush(Color.FromArgb(245, 200, 80)))
        {
            g.FillRectangle(tabBrush, 1, 2, 6, 3);
            g.FillRectangle(bodyBrush, 1, 4, 14, 10);
        }

        var file = new Bitmap(16, 16);
        using (Graphics g = Graphics.FromImage(file))
        using (var pageBrush = new SolidBrush(Color.White))
        using (var edgePen = new Pen(Color.FromArgb(120, 120, 120)))
        {
            g.FillRectangle(pageBrush, 3, 1, 10, 14);
            g.DrawRectangle(edgePen, 3, 1, 9, 13);
        }

        // Index order matches FolderIcon / FileIcon. The ImageList keeps using these bitmaps, so they are not disposed.
        list.Images.Add(folder);
        list.Images.Add(file);
        return list;
    }

    // ------------------------------------------------------------------ project tree

    FsNode? SelectedFs => _tree.SelectedNode?.Tag as FsNode;

    void LoadTree()
    {
        _tree.BeginUpdate();
        try
        {
            _tree.Nodes.Clear();
            TreeNode root = CreateDirectoryNode(_project.RootPath, _project.Name);
            _tree.Nodes.Add(root);
            PopulateDirectory(root);
            root.Expand();
            _tree.SelectedNode = root;
        }
        finally { _tree.EndUpdate(); }
    }

    static TreeNode CreateDirectoryNode(string path, string text)
    {
        var node = new TreeNode(text, FolderIcon, FolderIcon) { Tag = new FsNode(path, true) };
        node.Nodes.Add(new TreeNode("...") { Name = PlaceholderName }); // makes the expand arrow appear; replaced lazily
        return node;
    }

    static void PopulateDirectory(TreeNode node)
    {
        var fs = (FsNode)node.Tag!;
        node.Nodes.Clear();
        try
        {
            var dir = new DirectoryInfo(fs.Path);
            foreach (DirectoryInfo d in dir.EnumerateDirectories().OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase))
                node.Nodes.Add(CreateDirectoryNode(d.FullName, d.Name));
            foreach (FileInfo f in dir.EnumerateFiles().OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase))
                node.Nodes.Add(new TreeNode(f.Name, FileIcon, FileIcon) { Tag = new FsNode(f.FullName, false) });
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            node.Nodes.Add(new TreeNode($"({ex.Message})"));
        }
    }

    TreeNode? FindDirectoryNode(TreeNodeCollection nodes, string path)
    {
        foreach (TreeNode node in nodes)
        {
            if (node.Tag is not FsNode { IsDirectory: true } fs) continue;
            if (string.Equals(fs.Path, path, StringComparison.OrdinalIgnoreCase)) return node;
            if (FileUtil.IsSameOrUnder(path, fs.Path))
            {
                TreeNode? found = FindDirectoryNode(node.Nodes, path);
                if (found is not null) return found;
            }
        }
        return null;
    }

    void RefreshDirectory(string path)
    {
        TreeNode? node = FindDirectoryNode(_tree.Nodes, path);
        if (node is null) return;
        PopulateDirectory(node);
        node.Expand();
    }

    void RefreshSelected()
    {
        if (SelectedFs is not { } fs) return;
        RefreshDirectory(fs.IsDirectory ? fs.Path : Path.GetDirectoryName(fs.Path)!);
    }

    string TargetDirectory() => SelectedFs switch
    {
        null => _project.RootPath,
        { IsDirectory: true } dir => dir.Path,
        { } file => Path.GetDirectoryName(file.Path)!
    };

    void NewItem(bool isFolder)
    {
        string directory = TargetDirectory();
        string defaultName = _project.Language?.NewFileName ?? "untitled.txt";
        string? name = PromptForm.Ask(this, isFolder ? "New Folder" : "New File", "Name:", isFolder ? "" : defaultName);
        if (string.IsNullOrWhiteSpace(name)) return;

        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            MessageBox.Show(this, "That name contains characters that are not allowed.", "inpsIDE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string path = Path.Combine(directory, name);
        if (File.Exists(path) || Directory.Exists(path))
        {
            MessageBox.Show(this, $"'{name}' already exists.", "inpsIDE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            if (isFolder) Directory.CreateDirectory(path);
            else SimpleFileHandler.Write(path, LanguageRegistry.ForFile(path)?.NewFileContent(path) ?? "");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ShowError(ex);
            return;
        }

        RefreshDirectory(directory);
        if (!isFolder) OpenFile(path);
    }

    void DeleteSelected()
    {
        if (SelectedFs is not { } fs || _tree.SelectedNode?.Parent is null) return; // the root cannot be deleted

        string kind = fs.IsDirectory ? "folder (and everything in it)" : "file";
        var answer = MessageBox.Show(this, $"Permanently delete this {kind}?\n\n{fs.Path}", "Delete",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
        if (answer != DialogResult.Yes) return;

        foreach (EditorTab tab in Tabs.Where(t => FileUtil.IsSameOrUnder(t.FilePath, fs.Path)).ToList())
        {
            _tabs.TabPages.Remove(tab);
            tab.Dispose();
        }

        try
        {
            if (fs.IsDirectory) Directory.Delete(fs.Path, recursive: true);
            else File.Delete(fs.Path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ShowError(ex);
        }

        RefreshDirectory(Path.GetDirectoryName(fs.Path)!);
    }

    // ------------------------------------------------------------------ editor tabs

    void OpenInitialFile()
    {
        // Template entry file first, then the language's usual suspects (all languages if the project has none), then the README.
        IEnumerable<string> languageFiles = _project.Language?.InitialFileCandidates
            ?? LanguageRegistry.All.SelectMany(l => l.InitialFileCandidates);

        IEnumerable<string> candidates = new[] { _project.Template?.EntryFile }
            .OfType<string>()
            .Concat(languageFiles)
            .Append("README.md");

        foreach (string relative in candidates)
        {
            string path = Path.Combine(_project.RootPath, relative);
            if (File.Exists(path))
            {
                OpenFile(path);
                return;
            }
        }
    }

    void OpenFile(string path)
    {
        foreach (EditorTab existing in Tabs)
        {
            if (string.Equals(existing.FilePath, path, StringComparison.OrdinalIgnoreCase))
            {
                _tabs.SelectedTab = existing;
                return;
            }
        }

        try
        {
            if (new FileInfo(path).Length > MaxFileBytes)
            {
                MessageBox.Show(this, "This file is larger than 5 MB and cannot be opened in the editor.", "inpsIDE", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (FileUtil.IsBinary(path))
            {
                MessageBox.Show(this, "This looks like a binary file, so it was not opened.", "inpsIDE", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var tab = new EditorTab(path);
            tab.Editor.CaretMoved += (_, _) => { if (_tabs.SelectedTab == tab) UpdateCaretStatus(); };
            _tabs.TabPages.Add(tab);
            _tabs.SelectedTab = tab;
            tab.Editor.Focus();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ShowError(ex);
        }
    }

    bool TrySave(EditorTab tab)
    {
        try
        {
            tab.Editor.SaveFile(tab.FilePath);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ShowError(ex);
            return false;
        }
    }

    void SaveCurrent()
    {
        if (_tabs.SelectedTab is EditorTab tab && TrySave(tab)) SetStatus($"Saved {tab.FilePath}");
    }

    void SaveAll()
    {
        foreach (EditorTab tab in Tabs.Where(t => t.Editor.IsDirty).ToList())
            if (!TrySave(tab)) return;
        SetStatus("All files saved.");
    }

    void CloseCurrentTab()
    {
        if (_tabs.SelectedTab is EditorTab tab) CloseTab(tab);
    }

    /// <returns>false if the user cancelled.</returns>
    bool CloseTab(EditorTab tab)
    {
        if (tab.Editor.IsDirty)
        {
            _tabs.SelectedTab = tab;
            var answer = MessageBox.Show(this, $"Save changes to {Path.GetFileName(tab.FilePath)}?", "inpsIDE",
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (answer == DialogResult.Cancel) return false;
            if (answer == DialogResult.Yes && !TrySave(tab)) return false;
        }

        _tabs.TabPages.Remove(tab);
        tab.Dispose();
        return true;
    }

    void UpdateCaretStatus()
    {
        if (_tabs.SelectedTab is EditorTab tab)
        {
            (int line, int column) = tab.Editor.CaretPosition;
            _statusPosition.Text = $"Ln {line}, Col {column}";
        }
        else
        {
            _statusPosition.Text = "";
        }
    }

    // ------------------------------------------------------------------ running (language independent)

    /// <summary>The process that currently receives console input.</summary>
    ProcessRunner? ActiveProcess => _runner.IsRunning ? _runner : _server.IsRunning ? _server : null;

    void WireRunners()
    {
        // Output goes straight into the console's thread safe queue; no marshalling needed.
        _runner.OutputReceived += _console.Write;
        _server.OutputReceived += _console.Write;
        _runner.Exited += code => OnUi(() => OnProcessExited($"[Process exited with code {code}]"));
        _server.Exited += code => OnUi(() => OnProcessExited($"[Web server stopped, exit code {code}]"));

        _console.InputSubmitted += SendInput;
        _console.EndOfInputRequested += () => ActiveProcess?.CloseInput();
        _console.StopRequested += () =>
        {
            _runner.Stop();
            _server.Stop();
        };
    }

    void SendInput(string line)
    {
        if (ActiveProcess is not { } process) return;

        // The child's stdin is a pipe, so nothing echoes what was typed: do it here (queued after the prompt it answers).
        _console.WriteLine(line);
        process.SendLine(line);
    }

    void OnProcessExited(string message)
    {
        bool inputHadFocus = _console.InputFocused;
        _console.WriteLine(message);
        UpdateRunState();
        if (inputHadFocus && !_console.ProcessRunning) (_tabs.SelectedTab as EditorTab)?.Editor.Focus();
    }

    void RunCurrentFile()
    {
        if (_tabs.SelectedTab is not EditorTab tab)
        {
            SetStatus("Open a file first.");
            return;
        }
        if (_runner.IsRunning) return;

        // The file's own language decides how it is run, so a project may mix languages.
        LanguageSupport? language = LanguageRegistry.ForFile(tab.FilePath);
        if (language is null || !language.CanRun(tab.FilePath))
        {
            SetStatus("inpsIDE doesn't know how to run this kind of file.");
            return;
        }
        if (RequireRuntime(language) is not { } executable) return;
        if (tab.Editor.IsDirty && !TrySave(tab)) return;

        _console.Clear();
        Launch(_runner, language.CreateRunFileCommand(executable, _project.RootPath, tab.FilePath));
    }

    void StartServer()
    {
        if (_server.IsRunning) return;
        if (_project.Language is not { } language || !_serverCapable)
        {
            SetStatus("This project type has no web server.");
            return;
        }
        if (RequireRuntime(language) is not { } executable) return;

        SaveAll();
        if (language.CreateServerCommand(executable, _project.RootPath, _project.Template) is not { } command)
        {
            SetStatus("Could not start the web server: the project's entry file is missing.");
            return;
        }

        _serverUrl = command.Url;
        if (!Launch(_server, command)) return;
        if (command.Url is not { } url) return;

        _console.WriteLine($"Web server starting on {url}");
        if (command.OpenBrowserDelayMs <= 0)
        {
            OpenBrowser(url);
        }
        else
        {
            // Only open the browser if the server survived its start-up (e.g. Flask missing would end it at once).
            _ = Task.Delay(command.OpenBrowserDelayMs).ContinueWith(
                _ => OnUi(() => { if (_server.IsRunning) OpenBrowser(url); }),
                TaskScheduler.Default);
        }
    }

    void StopServer()
    {
        _server.Stop();
        UpdateRunState();
    }

    bool Launch(ProcessRunner runner, RunCommand command)
    {
        _console.WriteLine("> " + command.Display);
        try
        {
            runner.Start(command);
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            ShowError(ex);
            return false;
        }

        UpdateRunState();
        return true;
    }

    string? RequireRuntime(LanguageSupport language)
    {
        string? executable = language.FindExecutable(_project.RootPath);
        if (executable is null)
        {
            MessageBox.Show(this, language.MissingRuntimeMessage(_project.RootPath),
                $"{language.DisplayName} runtime not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        return executable;
    }

    static void OpenBrowser(string url)
    {
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch (Win32Exception) { /* no default browser registered - the URL is in the console anyway */ }
    }

    void UpdateRunState()
    {
        _runItem.Enabled = !_runner.IsRunning;
        _serverStartItem.Enabled = _serverCapable && !_server.IsRunning;
        _serverStopItem.Enabled = _server.IsRunning;
        _console.ProcessRunning = _runner.IsRunning || _server.IsRunning;

        SetStatus(_server.IsRunning
            ? (_serverUrl is null ? "Web server running" : $"Web server running on {_serverUrl}")
            : _runner.IsRunning ? "Running..." : "Ready");
    }

    // ------------------------------------------------------------------ helpers

    void SetStatus(string message) => _statusMessage.Text = message;

    void ShowError(Exception ex) =>
        MessageBox.Show(this, ex.Message, "inpsIDE", MessageBoxButtons.OK, MessageBoxIcon.Error);

    /// <summary>Marshals work from a background thread (process events) to the UI thread.</summary>
    void OnUi(Action action)
    {
        if (IsDisposed || !IsHandleCreated) return;
        try { BeginInvoke(action); }
        catch (InvalidOperationException) { /* form closed in the meantime */ }
    }

    void ExitApplication()
    {
        _exitRequested = true;
        Close();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (e.Cancel) return;

        foreach (EditorTab tab in Tabs.ToList())
        {
            if (!CloseTab(tab))
            {
                e.Cancel = true;
                _exitRequested = false;
                return;
            }
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        _runner.Dispose();
        _server.Dispose();

        if (_exitRequested) _startForm.Close(); // the start form is the app's main form, so this ends the program
        else _startForm.ShowAgain();
    }
}
