namespace inpsIDE
{
    partial class Editor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            editorMenuStrip = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            runToolStripMenuItem = new ToolStripMenuItem();
            fileListPanel = new Panel();
            fileListHint = new Label();
            fileList = new TreeView();
            codeEditorPanel = new Panel();
            codeEditor = new TextBox();
            newToolStripMenuItem = new ToolStripMenuItem();
            editorMenuStrip.SuspendLayout();
            fileListPanel.SuspendLayout();
            codeEditorPanel.SuspendLayout();
            SuspendLayout();
            // 
            // editorMenuStrip
            // 
            editorMenuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, runToolStripMenuItem });
            editorMenuStrip.Location = new Point(0, 0);
            editorMenuStrip.Name = "editorMenuStrip";
            editorMenuStrip.Padding = new Padding(7, 2, 0, 2);
            editorMenuStrip.Size = new Size(632, 24);
            editorMenuStrip.TabIndex = 0;
            editorMenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // runToolStripMenuItem
            // 
            runToolStripMenuItem.Name = "runToolStripMenuItem";
            runToolStripMenuItem.Size = new Size(40, 20);
            runToolStripMenuItem.Text = "Run";
            runToolStripMenuItem.Click += runToolStripMenuItem_Click;
            // 
            // fileListPanel
            // 
            fileListPanel.Controls.Add(fileListHint);
            fileListPanel.Controls.Add(fileList);
            fileListPanel.Dock = DockStyle.Left;
            fileListPanel.Location = new Point(0, 24);
            fileListPanel.Margin = new Padding(4, 3, 4, 3);
            fileListPanel.Name = "fileListPanel";
            fileListPanel.Size = new Size(233, 259);
            fileListPanel.TabIndex = 1;
            // 
            // fileListHint
            // 
            fileListHint.Dock = DockStyle.Fill;
            fileListHint.Location = new Point(0, 0);
            fileListHint.Margin = new Padding(4, 0, 4, 0);
            fileListHint.Name = "fileListHint";
            fileListHint.Size = new Size(233, 259);
            fileListHint.TabIndex = 0;
            fileListHint.Text = "No files available";
            fileListHint.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // fileList
            // 
            fileList.BackColor = SystemColors.Control;
            fileList.BorderStyle = BorderStyle.None;
            fileList.Dock = DockStyle.Fill;
            fileList.Location = new Point(0, 0);
            fileList.Margin = new Padding(4, 3, 4, 3);
            fileList.Name = "fileList";
            fileList.Size = new Size(233, 259);
            fileList.TabIndex = 1;
            // 
            // codeEditorPanel
            // 
            codeEditorPanel.Controls.Add(codeEditor);
            codeEditorPanel.Dock = DockStyle.Fill;
            codeEditorPanel.Location = new Point(233, 24);
            codeEditorPanel.Margin = new Padding(4, 3, 4, 3);
            codeEditorPanel.Name = "codeEditorPanel";
            codeEditorPanel.Size = new Size(399, 259);
            codeEditorPanel.TabIndex = 2;
            // 
            // codeEditor
            // 
            codeEditor.Dock = DockStyle.Fill;
            codeEditor.Font = new Font("Consolas", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            codeEditor.Location = new Point(0, 0);
            codeEditor.Margin = new Padding(4, 3, 4, 3);
            codeEditor.MaxLength = 2000000000;
            codeEditor.Multiline = true;
            codeEditor.Name = "codeEditor";
            codeEditor.ScrollBars = ScrollBars.Both;
            codeEditor.Size = new Size(399, 259);
            codeEditor.TabIndex = 0;
            // 
            // newToolStripMenuItem
            // 
            newToolStripMenuItem.Name = "newToolStripMenuItem";
            newToolStripMenuItem.Size = new Size(180, 22);
            newToolStripMenuItem.Text = "New";
            // 
            // Editor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(632, 283);
            Controls.Add(codeEditorPanel);
            Controls.Add(fileListPanel);
            Controls.Add(editorMenuStrip);
            MainMenuStrip = editorMenuStrip;
            Margin = new Padding(4, 3, 4, 3);
            Name = "Editor";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "inpsIDE";
            WindowState = FormWindowState.Maximized;
            FormClosing += Editor_FormClosing;
            editorMenuStrip.ResumeLayout(false);
            editorMenuStrip.PerformLayout();
            fileListPanel.ResumeLayout(false);
            codeEditorPanel.ResumeLayout(false);
            codeEditorPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip editorMenuStrip;
        private System.Windows.Forms.Panel fileListPanel;
        private System.Windows.Forms.TreeView fileList;
        private System.Windows.Forms.Label fileListHint;
        private System.Windows.Forms.Panel codeEditorPanel;
        private System.Windows.Forms.TextBox codeEditor;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem runToolStripMenuItem;
        private ToolStripMenuItem newToolStripMenuItem;
    }
}