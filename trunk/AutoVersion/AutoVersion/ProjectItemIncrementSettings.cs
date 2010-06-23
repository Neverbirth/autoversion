using AutoVersion.Design;

using PluginCore;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Text;
using System.Windows.Forms.Design;

#if NET_35
using System.Xml.Linq;
#else
using AutoVersion.Utils;
using System.Xml;
#endif

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

        private string _airDescriptorFilename = string.Empty;
        /// <summary>
        /// Gets or sets the AIR descriptor filename.
        /// </summary>
        /// <value>The full path to the AIR descriptor filename.</value>
        [Category("Increment Settings")]
        [DefaultValue("")]
        [DisplayName("AIR Descriptor File")]
        [Description("Change this setting if your application descriptor file is not the default application.xml.")]
        [EditorAttribute(typeof(FileNameEditor), typeof(UITypeEditor))]
        public string AirDescriptorFile
        {
            get { return _airDescriptorFilename; }
            set
            {
                _airDescriptorFilename = !string.IsNullOrEmpty(value) ? PluginBase.CurrentProject.GetRelativePath(value) : string.Empty;
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

#if NET_35

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
                _airDescriptorFilename = autoVersionElement.GetAttributeValue("airDescriptorFile", string.Empty);
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
                UpdateAirVersion = bool.Parse(autoVersionElement.GetAttributeValue("updateAirVersion", "false"));
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

            if (!string.IsNullOrEmpty(AirDescriptorFile))
                autoVersionElement.Add(new XAttribute("airDescriptorFile", AirDescriptorFile ));

            if (BuildAction != BuildActionType.Both)
                autoVersionElement.Add(new XAttribute("buildAction", BuildAction.ToString()));

            autoVersionElement.Add(new XAttribute("startDate", StartDate.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            autoVersionElement.Add(new XAttribute("incrementBeforeBuild", IncrementBeforeBuild.ToString()));
            autoVersionElement.Add(new XAttribute("smartUpdate", SmartUpdate.ToString()));
            autoVersionElement.Add(new XAttribute("useGlobalSettings", UseGlobalSettings.ToString()));
            autoVersionElement.Add(new XAttribute("updateAirVersion", UpdateAirVersion.ToString()));

            projectVersionDocument.Add(autoVersionElement);

            projectVersionDocument.Save(_versionFile);
        }

#else

        /// <summary>
        /// Loads the settings into this instance.
        /// </summary>
        public override void Load()
        {
            try
            {
                using (XmlTextReader projectVersionDocument = new XmlTextReader(_versionFile))
                {
                    projectVersionDocument.MoveToContent();

                    AutoUpdateVersionData = bool.Parse(XmlUtils.GetAttributeValue(projectVersionDocument, "autoUpdateVersionData", "false"));
                    _versionFilename = XmlUtils.GetAttributeValue(projectVersionDocument, "versionFilename", string.Empty);
                    _versionTemplateFilename = XmlUtils.GetAttributeValue(projectVersionDocument, "versionTemplateFilename", string.Empty);
                    _airDescriptorFilename = XmlUtils.GetAttributeValue(projectVersionDocument, "airDescriptorFile", string.Empty);
                    IncrementBeforeBuild = bool.Parse(XmlUtils.GetAttributeValue(projectVersionDocument, "incrementBeforeBuild", "true"));
                    UseGlobalSettings = bool.Parse(XmlUtils.GetAttributeValue(projectVersionDocument, "useGlobalSettings", (GlobalIncrementSettings.GetInstance().Apply == GlobalIncrementSettings.ApplyGlobalSettings.AsDefault).ToString()));

                    try
                    {
                        BuildAction = (BuildActionType)Enum.Parse(typeof(BuildActionType), XmlUtils.GetAttributeValue(projectVersionDocument, "buildAction", "Both"));
                    }
                    catch (ArgumentException)
                    {
                        BuildAction = BuildActionType.Both;
                    }

                    string versioningStyle = XmlUtils.GetAttributeValue(projectVersionDocument, "versioningStyle", VersioningStyle.GetDefaultGlobalVariable());

                    VersioningStyle.FromGlobalVariable(versioningStyle);
                    StartDate = DateTime.Parse(XmlUtils.GetAttributeValue(projectVersionDocument, "startDate", "10/21/1975 00:00:00"), System.Globalization.CultureInfo.InvariantCulture);
                    SmartUpdate = bool.Parse(XmlUtils.GetAttributeValue(projectVersionDocument, "smartUpdate", "false"));
                    UpdateAirVersion = bool.Parse(XmlUtils.GetAttributeValue(projectVersionDocument, "updateAirVersion", "false"));
                }

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
            using (XmlTextWriter projectVersionDocument = new XmlTextWriter(_versionFile, Encoding.UTF8))
            {

                projectVersionDocument.WriteStartDocument();
                projectVersionDocument.WriteStartElement("autoVersion");

                projectVersionDocument.WriteAttributeString("autoUpdateVersionData", AutoUpdateVersionData.ToString());
                projectVersionDocument.WriteAttributeString("versioningStyle", VersioningStyle.ToGlobalVariable());

                if (!string.IsNullOrEmpty(VersionFilename))
                    projectVersionDocument.WriteAttributeString("versionFilename", VersionFilename);

                if (!string.IsNullOrEmpty(VersionTemplateFilename))
                    projectVersionDocument.WriteAttributeString("versionTemplateFilename", VersionTemplateFilename);

                if (!string.IsNullOrEmpty(AirDescriptorFile))
                    projectVersionDocument.WriteAttributeString("airDescriptorFile", AirDescriptorFile);

                if (BuildAction != BuildActionType.Both)
                    projectVersionDocument.WriteAttributeString("buildAction", BuildAction.ToString());

                projectVersionDocument.WriteAttributeString("startDate", StartDate.ToString(System.Globalization.CultureInfo.InvariantCulture));
                projectVersionDocument.WriteAttributeString("incrementBeforeBuild", IncrementBeforeBuild.ToString());
                projectVersionDocument.WriteAttributeString("smartUpdate", SmartUpdate.ToString());
                projectVersionDocument.WriteAttributeString("useGlobalSettings", UseGlobalSettings.ToString());
                projectVersionDocument.WriteAttributeString("updateAirVersion", UpdateAirVersion.ToString());

                projectVersionDocument.WriteEndElement();
                projectVersionDocument.WriteEndDocument();
            }
        }

#endif

        #endregion
    }
}

