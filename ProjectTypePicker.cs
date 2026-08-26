using inpsNuGet;

namespace inpsIDE
{
    public partial class ProjectTypePicker : Form
    {
        public ProjectTypePicker()
        {
            InitializeComponent();

            projectTypeList.AddItem(new ClickableElement("inpsGE").SetEvent(() =>
            {
                MessageBox.Show("inpsGE project type selected");
            }));
        }

        private void createButton_Click(object sender, EventArgs e)
        {
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
