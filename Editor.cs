using inpsNuGet;
using System.Diagnostics;

namespace inpsIDE
{
    public partial class Editor : Form
    {
        MainCode MainCode;
        public string ProjectFile, ProjectFileContent, ProjectType, ProjectTitle, ProjectLocation;
        string CurrentFile = string.Empty;
        List<ToolStripMenuItem> NewFileTypes = [
            new ToolStripMenuItem("*.cs"),
            new ToolStripMenuItem("*.py"),
            new ToolStripMenuItem("*.txt")
            ];

        public Editor(MainCode MainCode, string ProjectFile)
        {
            InitializeComponent();

            this.ProjectFile = ProjectFile;
            this.MainCode = MainCode;
            MainCode.Hide();
            ProjectFileContent = SimpleFileHandler.Read(ProjectFile);
            ProjectType = ProjectFileContent.Split('\n')[0].Split('=')[1];
            ProjectTitle = ProjectFileContent.Split('\n')[1].Split('=')[1];
            ProjectLocation = ProjectFileContent.Split('\n')[2].Split('=')[1];
            Text = $"{ProjectTitle}";
            List<string> Recents = [.. SimpleFileHandler.Read("inpsIDE_recent").Split('\n')];
            if (Recents.Contains(ProjectFile))
            {
                Recents.Remove(ProjectFile);
            }
            Recents.Insert(0, ProjectFile);
            string NewRecents = string.Empty;
            foreach (string Recent in Recents)
            {
                NewRecents += Recent + "\n";
            }
            SimpleFileHandler.Write("inpsIDE_recent", NewRecents.Trim());

            foreach (ToolStripMenuItem ToolStripMenuItem in NewFileTypes)
            {
                ToolStripMenuItem.Click += (s, e) =>
                {
                    new NewFileDialog(this, ToolStripMenuItem.Text).ShowDialog();
                };
                newToolStripMenuItem.DropDownItems.Add(ToolStripMenuItem);
            }

            if (ProjectType == "Python")
            {
                ToolStripMenuItem Pip = new ToolStripMenuItem("pip");
                ToolStripMenuItem InstallPip = new ToolStripMenuItem("Install pip");
                ToolStripMenuItem InstallPipPackage = new ToolStripMenuItem("Install pip package");

                Pip.DropDownItems.Add(InstallPip);
                Pip.DropDownItems.Add(InstallPipPackage);

                editorMenuStrip.Items.Add(Pip);
            }

            RefreshList();
        }

        public void RefreshList()
        {
            fileList.Nodes.Clear();
            ProjectFileContent = SimpleFileHandler.Read(ProjectFile);
            string[] Files = ProjectFileContent.Split(["Files="], StringSplitOptions.None)[1].Split('\n');
            foreach (string File in Files)
            {
                if (!string.IsNullOrWhiteSpace(File))
                {
                    fileListHint.Visible = false;
                    fileList.Nodes.Add(File);
                }
            }
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ProjectType == "Python")
            {
                PyCS pyCS = new PyCS(ProjectLocation);
                Debug.WriteLine(pyCS.GetFileOutput($"{ProjectLocation}\\{CurrentFile}"));
            }
            else
            {
                new Actions().RunExe($"{ProjectLocation}\\{ProjectType}\\{ProjectType}.exe", $"title={ProjectTitle}");
            }
        }

        private void Editor_FormClosing(object sender, FormClosingEventArgs e)
        {
            MainCode.Close();
        }

        private void fileList_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (Text.Contains('*'))
            {
                if (MessageBox.Show("You have unsaved changes. Save changes to the current file?", "Unsaved changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    SimpleFileHandler.Write($"{ProjectLocation}\\{CurrentFile}", codeEditor.Text);
                    Text = $"{ProjectTitle} - {CurrentFile}";
                }
            }
            else
            {
                try
                {
                    codeEditor.Text = SimpleFileHandler.Read($"{ProjectLocation}\\{e.Node.Text}");
                    Text = $"{ProjectTitle} - {e.Node.Text}";
                    CurrentFile = e.Node.Text;
                }
                catch
                {
                    MessageBox.Show("This file could not be read.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SimpleFileHandler.Write($"{ProjectLocation}\\{CurrentFile}", codeEditor.Text);
            Text = $"{ProjectTitle} - {CurrentFile}";
        }

        private void codeEditor_TextChanged(object sender, EventArgs e)
        {
            Text = $"{ProjectTitle} - {CurrentFile}*";
        }
    }
}
