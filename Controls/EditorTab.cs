namespace inpsIDE.Controls;

/// <summary>A tab page hosting one <see cref="CodeEditor"/> for one file.</summary>
public sealed class EditorTab : TabPage
{
    public CodeEditor Editor { get; }
    public string FilePath { get; }

    public EditorTab(string filePath)
    {
        FilePath = filePath;
        Editor = new CodeEditor { Dock = DockStyle.Fill };
        Controls.Add(Editor);

        Editor.LoadFile(filePath);
        Editor.DirtyChanged += (_, _) => UpdateTitle();
        UpdateTitle();
    }

    void UpdateTitle() => Text = Path.GetFileName(FilePath) + (Editor.IsDirty ? " *" : "");
}
