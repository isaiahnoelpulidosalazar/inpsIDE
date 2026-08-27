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
            projectNameTextBox = new TextBox();
            label1 = new Label();
            label2 = new Label();
            projectPathTextBox = new TextBox();
            label3 = new Label();
            projectResultingDirectoryTextBox = new TextBox();
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
            projectTypeList.Size = new Size(680, 235);
            projectTypeList.TabIndex = 0;
            projectTypeList.WrapContents = false;
            // 
            // createButton
            // 
            createButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            createButton.Location = new Point(538, 385);
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
            cancelButton.Location = new Point(378, 385);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(154, 44);
            cancelButton.TabIndex = 3;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += cancelButton_Click;
            // 
            // projectNameTextBox
            // 
            projectNameTextBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            projectNameTextBox.Location = new Point(12, 268);
            projectNameTextBox.Name = "projectNameTextBox";
            projectNameTextBox.Size = new Size(680, 23);
            projectNameTextBox.TabIndex = 4;
            projectNameTextBox.TextChanged += projectNameTextBox_TextChanged;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(12, 250);
            label1.Name = "label1";
            label1.Size = new Size(77, 15);
            label1.TabIndex = 5;
            label1.Text = "Project name";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(12, 294);
            label2.Name = "label2";
            label2.Size = new Size(71, 15);
            label2.TabIndex = 6;
            label2.Text = "Project path";
            // 
            // projectPathTextBox
            // 
            projectPathTextBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            projectPathTextBox.Location = new Point(12, 312);
            projectPathTextBox.Name = "projectPathTextBox";
            projectPathTextBox.Size = new Size(680, 23);
            projectPathTextBox.TabIndex = 7;
            projectPathTextBox.TextChanged += projectPathTextBox_TextChanged;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Location = new Point(12, 338);
            label3.Name = "label3";
            label3.Size = new Size(106, 15);
            label3.TabIndex = 8;
            label3.Text = "Resulting directory";
            // 
            // projectResultingDirectoryTextBox
            // 
            projectResultingDirectoryTextBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            projectResultingDirectoryTextBox.Enabled = false;
            projectResultingDirectoryTextBox.Location = new Point(12, 356);
            projectResultingDirectoryTextBox.Name = "projectResultingDirectoryTextBox";
            projectResultingDirectoryTextBox.ReadOnly = true;
            projectResultingDirectoryTextBox.Size = new Size(680, 23);
            projectResultingDirectoryTextBox.TabIndex = 9;
            // 
            // ProjectTypePicker
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(704, 441);
            Controls.Add(projectResultingDirectoryTextBox);
            Controls.Add(label3);
            Controls.Add(projectPathTextBox);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(projectNameTextBox);
            Controls.Add(cancelButton);
            Controls.Add(createButton);
            Controls.Add(projectTypeList);
            Name = "ProjectTypePicker";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Select project type";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private inpsNuGet.VerticalList projectTypeList;
        private Button createButton;
        private Button cancelButton;
        private TextBox projectNameTextBox;
        private Label label1;
        private Label label2;
        private TextBox projectPathTextBox;
        private Label label3;
        private TextBox projectResultingDirectoryTextBox;
    }
}