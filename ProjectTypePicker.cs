using inpsNuGet;
using System.Reflection;

namespace inpsIDE
{
    public partial class ProjectTypePicker : Form
    {
        ClickableElement inpsGE = new ClickableElement("inpsGE");

        public ProjectTypePicker()
        {
            InitializeComponent();

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
                    string ZipName = $"{ce.GetTitle()}.zip";
                    string ZipPath = projectResultingDirectoryTextBox.Text;
                    SimpleFileHandler.ProjectToLocation(Assembly.GetExecutingAssembly(), ZipName, ZipPath);
                    SimpleFileHandler.ExtractZipSafe(ZipPath + "\\" + ZipName, ZipPath);
                    Close();
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
