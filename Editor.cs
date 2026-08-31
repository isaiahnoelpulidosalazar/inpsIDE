using inpsNuGet;
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
    public partial class Editor : Form
    {
        MainCode MainCode;
        string Title;

        public Editor(MainCode MainCode, string ProjectFile)
        {
            InitializeComponent();

            this.MainCode = MainCode;
            MainCode.Hide();
            Title = SimpleFileHandler.Read(ProjectFile).Split('\n')[1].Split('=')[1];
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
            new Actions(null).RunExe($"{Title}\\inpsGE\\inpsGE.exe", "title=Liferoad");
        }

        private void Editor_FormClosing(object sender, FormClosingEventArgs e)
        {
            MainCode.Close();
        }
    }
}
