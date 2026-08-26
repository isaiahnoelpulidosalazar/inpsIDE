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
    public partial class ProjectTypePicker : Form
    {
        public ProjectTypePicker()
        {
            InitializeComponent();

            projectTypeList.AddItem("inpsGE");
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
