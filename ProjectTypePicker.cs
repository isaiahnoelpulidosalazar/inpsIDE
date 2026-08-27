using inpsNuGet;

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
        }

        private void createButton_Click(object sender, EventArgs e)
        {
            foreach (ClickableElement ce in projectTypeList.Controls)
            {
                if (ce.IsToggled())
                {
                    MessageBox.Show(ce.GetTitle());
                }
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
