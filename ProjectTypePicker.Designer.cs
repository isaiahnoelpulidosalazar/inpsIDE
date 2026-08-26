namespace inpsIDE
{
    partial class ProjectTypePicker
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
            projectTypeList = new inpsNuGet.VerticalList();
            createButton = new Button();
            cancelButton = new Button();
            SuspendLayout();
            // 
            // projectTypeList
            // 
            projectTypeList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            projectTypeList.AutoScroll = true;
            projectTypeList.BorderStyle = BorderStyle.FixedSingle;
            projectTypeList.FlowDirection = FlowDirection.TopDown;
            projectTypeList.Location = new Point(12, 12);
            projectTypeList.Name = "projectTypeList";
            projectTypeList.Padding = new Padding(0, 0, 0, 3);
            projectTypeList.Size = new Size(314, 292);
            projectTypeList.TabIndex = 0;
            projectTypeList.WrapContents = false;
            // 
            // createButton
            // 
            createButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            createButton.Location = new Point(172, 310);
            createButton.Name = "createButton";
            createButton.Size = new Size(154, 44);
            createButton.TabIndex = 2;
            createButton.Text = "Create";
            createButton.UseVisualStyleBackColor = true;
            createButton.Click += createButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            cancelButton.Location = new Point(12, 310);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(154, 44);
            cancelButton.TabIndex = 3;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += cancelButton_Click;
            // 
            // ProjectTypePicker
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(338, 366);
            Controls.Add(cancelButton);
            Controls.Add(createButton);
            Controls.Add(projectTypeList);
            Name = "ProjectTypePicker";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ProjectTypePicker";
            ResumeLayout(false);
        }

        #endregion

        private inpsNuGet.VerticalList projectTypeList;
        private Button createButton;
        private Button cancelButton;
    }
}