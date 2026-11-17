using inpsNuGet;
using inpsIDE.Languages;
using inpsIDE.Models;
using inpsIDE.Services;

namespace inpsIDE.Forms;

public sealed class ProjectTypeForm : Form
{
    const string DefaultDescription = "Select a project type above.";

    readonly VerticalList _list = new() { Dock = DockStyle.Fill };
    readonly ComboBox _filter = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
    readonly Label _description = new() { Dock = DockStyle.Fill, Height = 48, ForeColor = SystemColors.GrayText, Text = DefaultDescription };
    readonly TextBox _name = new() { Anchor = AnchorStyles.Left | AnchorStyles.Right };
    readonly TextBox _location = new() { Anchor = AnchorStyles.Left | AnchorStyles.Right };
    readonly Button _create = new() { Text = "Create", Size = new Size(100, 32), Enabled = false };
    readonly Button _cancel = new() { Text = "Cancel", Size = new Size(100, 32), DialogResult = DialogResult.Cancel };

    ClickableElement? _selectedElement;
    string? _filterId;

    public ProjectTemplate? SelectedTemplate { get; private set; }
    public string ProjectName => _name.Text.Trim();
    public string ProjectLocation => _location.Text.Trim();

    public ProjectTypeForm()
    {
        Text = "Create new project";
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(620, 600);
        MinimumSize = new Size(520, 520);
        MaximizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        AcceptButton = _create;
        CancelButton = _cancel;

        _location.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "inpsIDE Projects");
        _name.Text = SuggestName(_location.Text);

        _filter.Items.Add("All languages");
        foreach (LanguageSupport language in LanguageRegistry.All)
        {
            _filter.Items.Add(language.DisplayName);
        }
        _filter.Items.Add("Other");
        _filter.SelectedIndex = 0;

        BuildUi();
        PopulateList();

        _filter.SelectedIndexChanged += (_, _) =>
        {
            int index = _filter.SelectedIndex;
            _filterId = index <= 0 ? null
                      : index <= LanguageRegistry.All.Count ? LanguageRegistry.All[index - 1].Id
                      : "";
            PopulateList();
        };
        _create.Click += (_, _) => TryAccept();
    }

    void BuildUi()
    {
        var header = new Label { Text = "Choose a project type", AutoSize = true, Font = new Font("Segoe UI Semibold", 12f), Margin = new Padding(3, 3, 3, 8) };

        var filterRow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false, Margin = new Padding(0, 0, 0, 6) };
        filterRow.Controls.Add(new Label { Text = "Language:", AutoSize = true, Margin = new Padding(3, 6, 8, 3) });
        filterRow.Controls.Add(_filter);

        var browse = new Button { Text = "Browse...", AutoSize = true };
        browse.Click += (_, _) =>
        {
            using var dialog = new FolderBrowserDialog { Description = "Choose where the project folder will be created", UseDescriptionForTitle = true, InitialDirectory = ExistingParent(_location.Text) };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _location.Text = dialog.SelectedPath;
            }
        };

        var fields = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 3, RowCount = 2 };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        fields.Controls.Add(new Label { Text = "Name:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 6, 12, 3) }, 0, 0);
        fields.Controls.Add(_name, 1, 0);
        fields.SetColumnSpan(_name, 2);
        fields.Controls.Add(new Label { Text = "Location:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 6, 12, 3) }, 0, 1);
        fields.Controls.Add(_location, 1, 1);
        fields.Controls.Add(browse, 2, 1);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Margin = new Padding(0, 12, 0, 0) };
        buttons.Controls.Add(_cancel);
        buttons.Controls.Add(_create);

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(12) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(header, 0, 0);
        root.Controls.Add(filterRow, 0, 1);
        root.Controls.Add(_list, 0, 2);
        root.Controls.Add(_description, 0, 3);
        root.Controls.Add(fields, 0, 4);
        root.Controls.Add(buttons, 0, 5);
        Controls.Add(root);
    }

    void PopulateList()
    {
        _list.SuspendLayout();
        foreach (Control c in _list.Controls.Cast<Control>().ToList())
        {
            c.Dispose();
        }

        _selectedElement = null;
        SelectedTemplate = null;
        _create.Enabled = false;
        _description.Text = DefaultDescription;

        foreach (ProjectTemplate template in TemplateCatalog.All.Where(t => _filterId is null || (t.LanguageId ?? "") == _filterId))
        {
            AddTemplate(template);
        }

        _list.ResumeLayout(true);
        _list.AutoScrollPosition = Point.Empty;
    }

    void AddTemplate(ProjectTemplate template)
    {
        var item = new ClickableElement(template.Title);
        item.SetEvent(() => SelectTemplate(template, item));
        _list.AddItem(item);
    }

    void SelectTemplate(ProjectTemplate template, ClickableElement element)
    {
        if (_selectedElement is not null && _selectedElement != element && _selectedElement.IsToggled())
        {
            _selectedElement.Toggle();
        }
        if (!element.IsToggled())
        {
            element.Toggle();
        }

        _selectedElement = element;
        SelectedTemplate = template;
        _description.Text = template.Description;
        _create.Enabled = true;
    }

    void TryAccept()
    {
        string? error = ProjectService.ValidateName(ProjectName);
        if (error is null && !Path.IsPathRooted(ProjectLocation))
        {
            error = "Please enter a full folder path as the location.";
        }
        if (error is null && SelectedTemplate is null)
        {
            error = "Please select a project type.";
        }

        if (error is null)
        {
            string target = Path.Combine(ProjectLocation, ProjectName);
            if (Directory.Exists(target) && Directory.EnumerateFileSystemEntries(target).Any())
            {
                error = $"'{target}' already exists and is not empty.";
            }
        }

        if (error is not null)
        {
            MessageBox.Show(this, error, "Create new project", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DialogResult = DialogResult.OK;
    }

    static string SuggestName(string location)
    {
        const string baseName = "MyProject";
        string name = baseName;
        for (int i = 2; Directory.Exists(Path.Combine(location, name)); i++)
        {
            name = baseName + i;
        }
        return name;
    }

    static string ExistingParent(string path)
    {
        try
        {
            string? dir = path;
            while (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                dir = Path.GetDirectoryName(dir);
            }
            if (!string.IsNullOrEmpty(dir))
            {
                return dir;
            }
        }
        catch (ArgumentException) { }
        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }
}
