namespace inpsIDE
{
    public partial class MainCode : Form
    {
        public MainCode()
        {
            InitializeComponent();
        }

        private void createProjectButton_Click(object sender, EventArgs e)
        {
            new ProjectTypePicker().Show();
        }

        private void openProjectButton_Click(object sender, EventArgs e)
        {

        }
    }
}
