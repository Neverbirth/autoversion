using AutoVersion.Design;

using PluginCore;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Text;
using System.Windows.Forms.Design;
using System.Xml.Linq;

namespace AutoVersion
{
    /// <summary>
    /// Abtract class containing all common increment settings (global/solutionitem).
    /// </summary>
    [DefaultProperty("VersioningStyle")]
    internal class ProjectItemIncrementSettings : BaseIncrementSettings
    {

        #region  Fields

        private string _versionFile;

        #endregion

        #region  Properties

        private string _versionFilename = string.Empty;
        /// <summary>
        /// Gets or sets the version info filename.
        /// </summary>
        /// <value>The version info filename.</value>
        [Category("Increment Settings")]
        [Description("Use this value if the version attributes aren't saved in the default file. ")]
        [DefaultValue("")]
        [DisplayName("Version Filename")]
        [EditorAttribute(typeof(FileNameEditorEx), typeof(UITypeEditor))]
        public string VersionFilename
        {
            get { return _versionFilename; }
            set
            {
                _versionFilename = !string.IsNullOrEmpty(value) ? PluginBase.CurrentProject.GetRelativePath(value) : string.Empty;
            }
        }

        private string _versionTemplateFilename = string.Empty;
        /// <summary>
        /// Gets or sets the version info filename.
        /// </summary>
        /// <value>The version info filename.</value>
        [Category("Increment Settings")]
        [Description("Use this value if you want to use a class different to the default one. ")]
        [DefaultValue("")]
        [DisplayName("Version Class Template")]
        [EditorAttribute(typeof(FileNameEditor), typeof(UITypeEditor))]
        public string VersionTemplateFilename
        {
            get { return _versionTemplateFilename; }
            set
            {
                _versionTemplateFilename = !string.IsNullOrEmpty(value) ? PluginBase.CurrentProject.GetRelativePath(value) : string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets if this project should use the global settings instead of it's own.
        /// </summary>
        /// <value>The value</value>
        [Category("Increment Settings"), Description("If the project should use the global settings instead of it's own."), DisplayName("Use Global Settings"), DefaultValue(false)]
        public bool UseGlobalSettings { get; set; }

        #endregion

        #region  Constructor

        public ProjectItemIncrementSettings()
        {
            _versionFile = Path.Combine(Path.GetDirectoryName(PluginBase.CurrentProject.ProjectPath),
                            Path.GetFileNameWithoutExtension(PluginBase.CurrentProject.ProjectPath) +
                            ".version");
        }

        #endregion

        #region  Methods

        /// <summary>
        /// Loads the settings into this instance.
        /// </summary>
        public override void Load()
        {
            try
            {
                XDocument projectVersionDocument;

                if (File.Exists(_versionFile))
                    projectVersionDocument = XDocument.Load(_versionFile);
                else
                    projectVersionDocument = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"),
                        new XElement("autoVersion"));

                XElement autoVersionElement = projectVersionDocument.Element("autoVersion");

                AutoUpdateVersionData = bool.Parse(autoVersionElement.GetAttributeValue("autoUpdateVersionData", "false"));
                _versionFilename = autoVersionElement.GetAttributeValue("versionFilename", string.Empty);
                _versionTemplateFilename = autoVersionElement.GetAttributeValue("versionTemplateFilename", string.Empty);
                IncrementBeforeBuild = bool.Parse(autoVersionElement.GetAttributeValue("incrementBeforeBuild", "true"));
                UseGlobalSettings = bool.Parse(autoVersionElement.GetAttributeValue("useGlobalSettings", (GlobalIncrementSettings.GetInstance().Apply == GlobalIncrementSettings.ApplyGlobalSettings.AsDefault).ToString()));

                try
                {
                    BuildAction = (BuildActionType)Enum.Parse(typeof(BuildActionType), autoVersionElement.GetAttributeValue("buildAction", "Both"));
                }
                catch (ArgumentException)
                {
                    BuildAction = BuildActionType.Both;
                }

                string versioningStyle = autoVersionElement.GetAttributeValue("versioningStyle", VersioningStyle.GetDefaultGlobalVariable());

                VersioningStyle.FromGlobalVariable(versioningStyle);
                StartDate = DateTime.Parse(autoVersionElement.GetAttributeValue("startDate", "10/21/1975 00:00:00"), System.Globalization.CultureInfo.InvariantCulture);
                SmartUpdate = bool.Parse(autoVersionElement.GetAttributeValue("smartUpdate", "false"));

            }
            catch (Exception ex)
            {
                throw;
                //Logger.Write("Error occured while reading BuildVersionIncrement settings from \"" + SolutionItem.Filename + "\"\n" + ex.ToString(), LogLevel.Error);
            }
        }

        /// <summary>
        /// Saves the settings of this instance.
        /// </summary>
        public override void Save()
        {
            XDocument projectVersionDocument = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"));

            XElement autoVersionElement = new XElement("autoVersion");

            autoVersionElement.Add(new XAttribute("autoUpdateVersionData", AutoUpdateVersionData.ToString()));
            autoVersionElement.Add(new XAttribute("versioningStyle", VersioningStyle.ToGlobalVariable()));

            if (!string.IsNullOrEmpty(VersionFilename))
                autoVersionElement.Add(new XAttribute("versionFilename", VersionFilename));

            if (!string.IsNullOrEmpty(VersionTemplateFilename))
                autoVersionElement.Add(new XAttribute("versionTemplateFilename", VersionTemplateFilename));

            if (BuildAction != BuildActionType.Both)
                autoVersionElement.Add(new XAttribute("buildAction", BuildAction.ToString()));

            autoVersionElement.Add(new XAttribute("startDate", StartDate.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            autoVersionElement.Add(new XAttribute("incrementBeforeBuild", IncrementBeforeBuild.ToString()));
            autoVersionElement.Add(new XAttribute("smartUpdate", SmartUpdate.ToString()));
            autoVersionElement.Add(new XAttribute("useGlobalSettings", UseGlobalSettings.ToString()));

            projectVersionDocument.Add(autoVersionElement);

            projectVersionDocument.Save(_versionFile);
        }

        #endregion
    }
}

