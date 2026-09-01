using inpsNuGet;

namespace inpsIDE
{
    public partial class Editor : Form
    {
        MainCode MainCode;
        string ProjectFileContent, Title;

        public Editor(MainCode MainCode, string ProjectFile)
        {
            InitializeComponent();

            this.MainCode = MainCode;
            MainCode.Hide();
            ProjectFileContent = SimpleFileHandler.Read(ProjectFile);
            Title = ProjectFileContent.Split('\n')[1].Split('=')[1];
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
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string ProjectType = ProjectFileContent.Split('\n')[0].Split('=')[1];
            string ProjectLocation = ProjectFileContent.Split('\n')[2].Split('=')[1];
            new Actions(null).RunExe($"{ProjectLocation}\\{ProjectType}\\{ProjectType}.exe", $"title={Title}");
        }

        private void Editor_FormClosing(object sender, FormClosingEventArgs e)
        {
            MainCode.Close();
        }
    }
}
