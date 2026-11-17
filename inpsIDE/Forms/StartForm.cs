using inpsNuGet;
using inpsIDE.Languages;
using inpsIDE.Models;
using inpsIDE.Services;

namespace inpsIDE.Forms;

public sealed class StartForm : Form
{
    readonly VerticalList _recent = new() { Dock = DockStyle.Fill };
    readonly Label _emptyLabel = new() { Text = "No recent projects yet.", AutoSize = true, ForeColor = SystemColors.GrayText, Margin = new Padding(3, 0, 3, 6) };
    readonly Label _status = new() { AutoSize = true, ForeColor = SystemColors.GrayText };
    readonly TableLayoutPanel _left = new() { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(12) };
    readonly ToolTip _tip = new();

    public StartForm()
    {
        Text = "inpsIDE";
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(900, 560);
        MinimumSize = new Size(760, 480);
        StartPosition = FormStartPosition.CenterScreen;

        BuildUi();
        RefreshRecents();
    }

    void BuildUi()
    {
        var header = new Label { Text = "Recent projects", AutoSize = true, Font = new Font("Segoe UI Semibold", 13f), Margin = new Padding(3, 3, 3, 8) };
        var createButton = new Button { Text = "Create new project", Dock = DockStyle.Fill, Height = 42, Margin = new Padding(3, 8, 3, 3) };
        var openButton = new Button { Text = "Open existing project", Dock = DockStyle.Fill, Height = 42 };
        createButton.Click += async (_, _) => await CreateProjectAsync();
        openButton.Click += (_, _) => OpenExistingProject();

        _left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _left.Controls.Add(header, 0, 0);
        _left.Controls.Add(_emptyLabel, 0, 1);
        _left.Controls.Add(_recent, 0, 2);
        _left.Controls.Add(createButton, 0, 3);
        _left.Controls.Add(openButton, 0, 4);

        var title = new Label { Text = "inpsIDE", AutoSize = true, Font = new Font("Segoe UI Semibold", 32f) };
        var subtitle = new Label
        {
            Text = "The official IDE made by inpsStudio, written in C# and Windows Forms on .NET 10.",
            AutoSize = true, MaximumSize = new Size(520, 0), Font = new Font("Segoe UI", 11f), Margin = new Padding(6, 6, 3, 3)
        };
        var hint = new Label
        {
            Text = "Create a new project to get a bundled runtime, or open an existing folder.",
            AutoSize = true, MaximumSize = new Size(520, 0), ForeColor = SystemColors.GrayText, Margin = new Padding(6, 18, 3, 3)
        };

        var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(48, 56, 48, 24), BackColor = Color.FromArgb(245, 245, 248) };
        right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.Controls.Add(title, 0, 0);
        right.Controls.Add(subtitle, 0, 1);
        right.Controls.Add(hint, 0, 2);
        right.Controls.Add(_status, 0, 4);

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 380));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(_left, 0, 0);
        root.Controls.Add(right, 1, 0);
        Controls.Add(root);
    }

    public void RefreshRecents()
    {
        _recent.SuspendLayout();
        foreach (Control c in _recent.Controls.Cast<Control>().ToList())
        {
            c.Dispose();
        }

        List<RecentProject> items = RecentProjectsStore.Load();
        _emptyLabel.Visible = items.Count == 0;

        foreach (RecentProject project in items)
        {
            var element = new ClickableElement($"{project.Name}   -   {project.Path}");
            element.SetEvent(() => OpenRecent(project));
            foreach (Control c in element.Controls.Cast<Control>().Append(element))
            {
                _tip.SetToolTip(c, project.Path);
            }
            _recent.AddItem(element);
        }

        _recent.ResumeLayout(true);
        _recent.AutoScrollPosition = Point.Empty;
    }

    public void ShowAgain()
    {
        RefreshRecents();
        if (WindowState == FormWindowState.Minimized)
        {
            WindowState = FormWindowState.Normal;
        }
        Show();
        Activate();
    }

    void OpenRecent(RecentProject project)
    {
        if (!Directory.Exists(project.Path))
        {
            var answer = MessageBox.Show(this,
                $"The folder no longer exists:\n{project.Path}\n\nRemove it from the recent list?",
                "inpsIDE", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (answer == DialogResult.Yes)
            {
                RecentProjectsStore.Remove(project.Path);
                BeginInvoke(RefreshRecents);
            }
            return;
        }

        TryOpen(project.Path);
    }

    void OpenExistingProject()
    {
        using var dialog = new FolderBrowserDialog { Description = "Select a project folder", UseDescriptionForTitle = true };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            TryOpen(dialog.SelectedPath);
        }
    }

    void TryOpen(string folder)
    {
        try
        {
            OpenProject(ProjectService.Open(folder));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, ex.Message, "Could not open project", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    async Task CreateProjectAsync()
    {
        using var picker = new ProjectTypeForm();
        if (picker.ShowDialog(this) != DialogResult.OK || picker.SelectedTemplate is not { } template)
        {
            return;
        }

        string name = picker.ProjectName, location = picker.ProjectLocation;
        LanguageSupport? language = template.Language;
        SetBusy(true, language is null ? "Creating project..." : $"Creating project and setting up {language.DisplayName} (this can take a moment)...");
        try
        {
            IdeProject project = await Task.Run(() => ProjectService.Create(template, name, location));

            if (project.SetupWarning is { } warning)
            {
                MessageBox.Show(this, warning, "Project created with problems", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            OpenProject(project);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            MessageBox.Show(this, ex.Message, "Could not create project", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false, "");
        }
    }

    void SetBusy(bool busy, string message)
    {
        _left.Enabled = !busy;
        UseWaitCursor = busy;
        _status.Text = message;
    }

    void OpenProject(IdeProject project)
    {
        RecentProjectsStore.Add(project);
        var main = new MainForm(project, this);
        Hide();
        main.Show();
    }
}
