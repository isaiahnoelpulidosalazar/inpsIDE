namespace inpsIDE
{
    partial class MainCode
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            verticalList1 = new inpsNuGet.VerticalList();
            createProjectButton = new Button();
            openProjectButton = new Button();
            SuspendLayout();
            // 
            // verticalList1
            // 
            verticalList1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            verticalList1.AutoScroll = true;
            verticalList1.BorderStyle = BorderStyle.FixedSingle;
            verticalList1.FlowDirection = FlowDirection.TopDown;
            verticalList1.Location = new Point(12, 12);
            verticalList1.Name = "verticalList1";
            verticalList1.Padding = new Padding(0, 0, 0, 3);
            verticalList1.Size = new Size(390, 321);
            verticalList1.TabIndex = 0;
            verticalList1.WrapContents = false;
            // 
            // createProjectButton
            // 
            createProjectButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            createProjectButton.Location = new Point(408, 12);
            createProjectButton.Name = "createProjectButton";
            createProjectButton.Size = new Size(198, 44);
            createProjectButton.TabIndex = 1;
            createProjectButton.Text = "Create new project";
            createProjectButton.UseVisualStyleBackColor = true;
            createProjectButton.Click += createProjectButton_Click;
            // 
            // openProjectButton
            // 
            openProjectButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            openProjectButton.Location = new Point(408, 62);
            openProjectButton.Name = "openProjectButton";
            openProjectButton.Size = new Size(198, 44);
            openProjectButton.TabIndex = 2;
            openProjectButton.Text = "Open existing project";
            openProjectButton.UseVisualStyleBackColor = true;
            openProjectButton.Click += openProjectButton_Click;
            // 
            // MainCode
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(618, 345);
            Controls.Add(openProjectButton);
            Controls.Add(createProjectButton);
            Controls.Add(verticalList1);
            Name = "MainCode";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "inpsIDE";
            ResumeLayout(false);
        }

        #endregion

        private inpsNuGet.VerticalList verticalList1;
        private Button createProjectButton;
        private Button openProjectButton;
    }
}
