using inpsNuGet;

namespace inpsIDE
{
    public partial class Editor : Form
    {
        MainCode MainCode;
        string ProjectFileContent, ProjectType, Title, ProjectLocation;

        public Editor(MainCode MainCode, string ProjectFile)
        {
            InitializeComponent();

            this.MainCode = MainCode;
            MainCode.Hide();
            ProjectFileContent = SimpleFileHandler.Read(ProjectFile);
            ProjectType = ProjectFileContent.Split('\n')[0].Split('=')[1];
            Title = ProjectFileContent.Split('\n')[1].Split('=')[1];
            ProjectLocation = ProjectFileContent.Split('\n')[2].Split('=')[1];
            Text = $"inpsIDE - {Title}";
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

            if (ProjectType == "Python")
            {
                ToolStripMenuItem Pip = new ToolStripMenuItem("pip");
                ToolStripMenuItem InstallPip = new ToolStripMenuItem("Install pip");
                ToolStripMenuItem InstallPipPackage = new ToolStripMenuItem("Install pip package");

                Pip.DropDownItems.Add(InstallPip);
                Pip.DropDownItems.Add(InstallPipPackage);

                editorMenuStrip.Items.Add(Pip);
            }
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ProjectType == "Python")
            {
                PyCS pyCS = new PyCS(false, ProjectLocation);
            }
            else
            {
                new Actions().RunExe($"{ProjectLocation}\\{ProjectType}\\{ProjectType}.exe", $"title={Title}");
            }
        }

        private void Editor_FormClosing(object sender, FormClosingEventArgs e)
        {
            MainCode.Close();
        }
    }
}
