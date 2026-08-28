using inpsNuGet;

namespace inpsIDE
{
    public partial class MainCode : Form
    {
        public MainCode()
        {
            InitializeComponent();

            if (File.Exists("inpsIDE_recent"))
            {
                string RecentProjects = SimpleFileHandler.Read("inpsIDE_recent");

                foreach (string project in RecentProjects.Split('\n'))
                {
                    ClickableElement ce = new ClickableElement(project);
                    ce.SetEvent(() =>
                    {
                        new Editor(this, project).Show();
                    });
                    recentProjectList.AddItem(ce);
                }
            }
            else
            {
                File.Create("inpsIDE_recent").Close();
            }
        }

        private void createProjectButton_Click(object sender, EventArgs e)
        {
            new ProjectTypePicker(this).Show();
        }

        private void openProjectButton_Click(object sender, EventArgs e)
        {

        }
    }
}
