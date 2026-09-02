using inpsNuGet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace inpsIDE
{
    public partial class NewFileDialog : Form
    {
        Editor Editor;
        string? FileType, ProjectFile;

        public NewFileDialog(Editor Editor, string? FileType, string? ProjectFile)
        {
            InitializeComponent();

            this.Editor = Editor;
            this.FileType = FileType;
            this.ProjectFile = ProjectFile;
            Text = $"Create new {FileType} file";
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void createButton_Click(object sender, EventArgs e)
        {
            SimpleFileHandler.Append(ProjectFile, $"{fileNameTextBox.Text}{FileType.Substring(1)}\n");
            Editor.RefreshList();
            Close();
        }
    }
}
