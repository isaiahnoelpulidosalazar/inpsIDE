using inpsNuGet;

namespace inpsIDE
{
    public partial class NewFileDialog : Form
    {
        Editor Editor;
        string? FileType;

        public NewFileDialog(Editor Editor, string? FileType)
        {
            InitializeComponent();

            this.Editor = Editor;
            this.FileType = FileType;
            Text = $"Create new {FileType} file";
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void createButton_Click(object sender, EventArgs e)
        {
            SimpleFileHandler.Append(Editor.ProjectFile, $"{fileNameTextBox.Text}{FileType?.Substring(1)}\n");
            SimpleFileHandler.Write($"{Editor.ProjectLocation}\\{fileNameTextBox.Text}{FileType?.Substring(1)}", string.Empty);
            Editor.RefreshList();
            Close();
        }
    }
}
