using AutoVersion.Resources;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AutoVersion
{
    public partial class VersionDialog : Form
    {

        #region  Fields

        private ProjectItem _projectItem;

        #endregion

        #region  Constructor

        public VersionDialog()
        {
            InitializeComponent();

            this.Text = LocaleHelper.GetString("Title.VersionDialog");
            this.ButtonCancel.Text = LocaleHelper.GetString("Label.CancelButton");
            this.ButtonOk.Text = LocaleHelper.GetString("Label.OkButton");
            this.LabelVersion.Text = LocaleHelper.GetString("Label.Version");
        }

        #endregion

        #region  Event Handlers

        private void VersionDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
            {
                this._projectItem.Version = new Version(this.SimpleNumericTextBoxMajor.Text +
                    "." + this.SimpleNumericTextBoxMinor.Text +
                    "." + this.SimpleNumericTextBoxBuild.Text +
                    "." + this.SimpleNumericTextBoxRevision.Text);
                this._projectItem.SaveVersion();
                this._projectItem.IncrementSettings.Save();
            }
        }

        private void VersionDialog_Load(object sender, EventArgs e)
        {
            this._projectItem = new ProjectItem();
            this.PropertyGridSettings.SelectedObject = this._projectItem.IncrementSettings;
            this.SimpleNumericTextBoxMajor.Text = this._projectItem.Version.Major.ToString();
            this.SimpleNumericTextBoxMinor.Text = this._projectItem.Version.Minor.ToString();
            this.SimpleNumericTextBoxBuild.Text = this._projectItem.Version.Build.ToString();
            this.SimpleNumericTextBoxRevision.Text = this._projectItem.Version.Revision.ToString();
        }

        #endregion

    }
}
