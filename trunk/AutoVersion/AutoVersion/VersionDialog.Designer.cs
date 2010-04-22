namespace AutoVersion
{
    partial class VersionDialog
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
            this.LabelVersion = new System.Windows.Forms.Label();
            this.PropertyGridSettings = new System.Windows.Forms.PropertyGrid();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.ButtonOk = new System.Windows.Forms.Button();
            this.SimpleNumericTextBoxBuild = new Controls.SimpleNumericTextBox();
            this.SimpleNumericTextBoxMinor = new Controls.SimpleNumericTextBox();
            this.SimpleNumericTextBoxMajor = new Controls.SimpleNumericTextBox();
            this.SimpleNumericTextBoxRevision = new Controls.SimpleNumericTextBox();
            this.SuspendLayout();
            // 
            // LabelVersion
            // 
            this.LabelVersion.AutoSize = true;
            this.LabelVersion.Location = new System.Drawing.Point(12, 19);
            this.LabelVersion.Name = "LabelVersion";
            this.LabelVersion.Size = new System.Drawing.Size(45, 13);
            this.LabelVersion.TabIndex = 4;
            this.LabelVersion.Text = "Version:";
            // 
            // PropertyGridSettings
            // 
            this.PropertyGridSettings.Location = new System.Drawing.Point(15, 48);
            this.PropertyGridSettings.Name = "PropertyGridSettings";
            this.PropertyGridSettings.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.PropertyGridSettings.Size = new System.Drawing.Size(267, 178);
            this.PropertyGridSettings.TabIndex = 5;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(120, 232);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(75, 23);
            this.ButtonCancel.TabIndex = 6;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // ButtonOk
            // 
            this.ButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ButtonOk.Location = new System.Drawing.Point(208, 232);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(75, 23);
            this.ButtonOk.TabIndex = 7;
            this.ButtonOk.Text = "Ok";
            this.ButtonOk.UseVisualStyleBackColor = true;
            // 
            // SimpleNumericTextBoxBuild
            // 
            this.SimpleNumericTextBoxBuild.Location = new System.Drawing.Point(176, 16);
            this.SimpleNumericTextBoxBuild.Name = "SimpleNumericTextBoxBuild";
            this.SimpleNumericTextBoxBuild.Size = new System.Drawing.Size(50, 20);
            this.SimpleNumericTextBoxBuild.TabIndex = 3;
            // 
            // SimpleNumericTextBoxMinor
            // 
            this.SimpleNumericTextBoxMinor.AcceptsReturn = true;
            this.SimpleNumericTextBoxMinor.Location = new System.Drawing.Point(120, 16);
            this.SimpleNumericTextBoxMinor.Name = "SimpleNumericTextBoxMinor";
            this.SimpleNumericTextBoxMinor.Size = new System.Drawing.Size(50, 20);
            this.SimpleNumericTextBoxMinor.TabIndex = 2;
            // 
            // SimpleNumericTextBoxMajor
            // 
            this.SimpleNumericTextBoxMajor.Location = new System.Drawing.Point(64, 16);
            this.SimpleNumericTextBoxMajor.Name = "SimpleNumericTextBoxMajor";
            this.SimpleNumericTextBoxMajor.Size = new System.Drawing.Size(50, 20);
            this.SimpleNumericTextBoxMajor.TabIndex = 1;
            // 
            // SimpleNumericTextBoxRevision
            // 
            this.SimpleNumericTextBoxRevision.Location = new System.Drawing.Point(232, 16);
            this.SimpleNumericTextBoxRevision.Name = "SimpleNumericTextBoxRevision";
            this.SimpleNumericTextBoxRevision.Size = new System.Drawing.Size(50, 20);
            this.SimpleNumericTextBoxRevision.TabIndex = 0;
            // 
            // VersionDialog
            // 
            this.AcceptButton = this.ButtonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.ButtonOk);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.PropertyGridSettings);
            this.Controls.Add(this.LabelVersion);
            this.Controls.Add(this.SimpleNumericTextBoxBuild);
            this.Controls.Add(this.SimpleNumericTextBoxMinor);
            this.Controls.Add(this.SimpleNumericTextBoxMajor);
            this.Controls.Add(this.SimpleNumericTextBoxRevision);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VersionDialog";
            this.Text = "Project Version";
            this.Load += new System.EventHandler(this.VersionDialog_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VersionDialog_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.SimpleNumericTextBox SimpleNumericTextBoxRevision;
        private Controls.SimpleNumericTextBox SimpleNumericTextBoxMajor;
        private Controls.SimpleNumericTextBox SimpleNumericTextBoxMinor;
        private Controls.SimpleNumericTextBox SimpleNumericTextBoxBuild;
        private System.Windows.Forms.Label LabelVersion;
        private System.Windows.Forms.PropertyGrid PropertyGridSettings;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Button ButtonOk;
    }
}