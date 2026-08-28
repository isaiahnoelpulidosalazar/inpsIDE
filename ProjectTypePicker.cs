using inpsNuGet;
using System.Reflection;

namespace inpsIDE
{
    public partial class ProjectTypePicker : Form
    {
        MainCode MainCode;

        ClickableElement inpsGE = new ClickableElement("inpsGE");

        public ProjectTypePicker(MainCode MainCode)
        {
            InitializeComponent();

            this.MainCode = MainCode;

            inpsGE.SetEvent(() => { inpsGE.Toggle(); });

            projectTypeList.AddItem(inpsGE);

            projectNameTextBox.Text = "Untitled";
            projectPathTextBox.Text = Directory.GetCurrentDirectory();
            projectResultingDirectoryTextBox.Text = projectPathTextBox.Text + "\\" + projectNameTextBox.Text;
        }

        private void createButton_Click(object sender, EventArgs e)
        {
            foreach (ClickableElement ce in projectTypeList.Controls)
            {
                if (ce.IsToggled())
                {
                    string ProjectName = projectNameTextBox.Text;
                    string ZipName = $"{ce.GetTitle()}.zip";
                    string ZipPath = projectResultingDirectoryTextBox.Text + "\\" + ce.GetTitle();
                    string ProjectFile = $"{projectResultingDirectoryTextBox.Text}\\{ProjectName}.inpsproj";
                    string ProjectContent = $"inpsIDEProjectType={ce.GetTitle()}\n" +
                        $"Title={ProjectName}\n" +
                        $"Location={projectResultingDirectoryTextBox.Text}\n" +
                        $"Files=\n";
                    SimpleFileHandler.ProjectToLocationThenExtractZipThenDelete(Assembly.GetExecutingAssembly(), ZipName, ZipPath);
                    SimpleFileHandler.Write(ProjectFile, ProjectContent);
                    Close();
                    MainCode.Hide();
                    new Editor(MainCode, ProjectFile).Show();
                }
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void projectNameTextBox_TextChanged(object sender, EventArgs e)
        {
            projectResultingDirectoryTextBox.Text = projectPathTextBox.Text + "\\" + projectNameTextBox.Text;
        }

        private void projectPathTextBox_TextChanged(object sender, EventArgs e)
        {
            projectResultingDirectoryTextBox.Text = projectPathTextBox.Text + "\\" + projectNameTextBox.Text;
        }
    }
}
