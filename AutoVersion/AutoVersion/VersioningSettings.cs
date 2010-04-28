﻿using AutoVersion.Design;

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
    internal class VersioningSettings
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
                _versionFilename = !string.IsNullOrEmpty(value) ? Utils.IOUtils.MakeRelativePath(PluginBase.CurrentProject.ProjectPath, value) : string.Empty;
            }
        }

        private string _versionFilePackage = string.Empty;
        /// <summary>
        /// Gets or sets the version file package level.
        /// </summary>
        /// <value>The package name.</value>
        [Category("Increment Settings")]
        [Description("Use this value if the version is not located at the root of a class path. ")]
        [DefaultValue("")]
        [DisplayName("Version File Package")]
        public string VersionFilePackage
        {
            get { return _versionFilePackage; }
            set
            {
                _versionFilePackage = value ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to auto update the file version.
        /// </summary>
        /// <value>
        /// 	<c>true</c> to auto update the file version; otherwise, <c>false</c>.
        /// </value>
        [Category("Increment Settings"), Description("Auto update the version data."), DisplayName("Update version data"), DefaultValue(false)]
        public bool AutoUpdateVersionData { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>The start date.</value>
        [Category("Increment Settings"), Description("The start date to use."), DisplayName("Start Date"), DefaultValue(typeof(DateTime), "1975/10/21")]
        public DateTime StartDate { get; set; }

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
                _versionTemplateFilename = !string.IsNullOrEmpty(value) ? Utils.IOUtils.MakeRelativePath(PluginBase.CurrentProject.ProjectPath, value) : string.Empty;
            }
        }

        private bool _incrementBeforeBuild;
        /// <summary>
        /// Gets or set if the increment should happen before or after the current build.
        /// </summary>
        /// <remarks>WorkItem 3589 from PeteBSC</remarks>
        /// <value>The new value for this property.</value>
        [Category("Condition")]
        [Description("If the increment should be executed before the build. Incrementing after build is complete doesn't work with Flash IDE projects.")]
        [DisplayName("Increment Before Build")]
        [DefaultValue(true)]
        public bool IncrementBeforeBuild
        {
            get { return _incrementBeforeBuild; }
            set { _incrementBeforeBuild = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is using UTC.
        /// </summary>
        /// <value><c>true</c> if this instance is using UTC; otherwise, <c>false</c>.</value>
        [Category("Increment Settings"), Description("Indicates wheter to use Coordinated Universal Time (UTC) time stamps."), DisplayName("Use Coordinated Universal Time"), DefaultValue(false)]
        public bool IsUniversalTime { get; set; }

        private VersioningStyle _versioningStyle = new VersioningStyle();
        /// <summary>
        /// Gets or sets the increment settings.
        /// </summary>
        /// <value>The increment settings.</value>
        [Browsable(true)]
        [Category("Increment Settings")]
        [DisplayName("Versioning Style")]
        [Description("The version increment style settings.")]
        public VersioningStyle VersioningStyle
        {
            get { return this._versioningStyle; }
            set { this._versioningStyle = value; }
        }

        private bool ShouldSerializeVersioningStyle()
        {
            return _versioningStyle.ToString() != "None.None.None.None";
        }

        /// <summary>
        /// Gets or sets the build action
        /// </summary>
        /// <value>The build action on which the auto update should occur.</value>
        [Category("Condition"), DefaultValue(BuildActionType.Both), DisplayName("Build Action"), Description("Set this to the desired build action when the auto update should occur.")]
        public BuildActionType BuildAction { get; set; }

        #endregion

        #region  Constructor

        public VersioningSettings()
        {
            _versionFile = Path.Combine(Path.GetDirectoryName(PluginBase.CurrentProject.ProjectPath),
                            Path.GetFileNameWithoutExtension(PluginBase.CurrentProject.ProjectPath) +
                            ".version");
        }

        #endregion

        #region  Methods

        private string GetProjectVariable(XElement baseElement, string attributeName, string defaultValue)
        {
            XAttribute attribute = baseElement.Attribute(attributeName);
            return attribute != null ? attribute.Value : defaultValue;
        }

        /// <summary>
        /// Loads the settings into this instance.
        /// </summary>
        public void Load()
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

                AutoUpdateVersionData = bool.Parse(GetProjectVariable(autoVersionElement, "autoUpdateVersionData", "false"));
                VersionFilename = GetProjectVariable(autoVersionElement, "versionFilename", string.Empty);
                VersionFilePackage = GetProjectVariable(autoVersionElement, "versionFilePackage", string.Empty);
                VersionTemplateFilename = GetProjectVariable(autoVersionElement, "versionTemplateFilename", string.Empty);
                IncrementBeforeBuild = bool.Parse(GetProjectVariable(autoVersionElement, "incrementBeforeBuild", "true"));

                try
                {
                    BuildAction = (BuildActionType)Enum.Parse(typeof(BuildActionType), GetProjectVariable(autoVersionElement, "buildAction", "Both"));
                }
                catch (ArgumentException)
                {
                    BuildAction = BuildActionType.Both;
                }

                string versioningStyle = GetProjectVariable(autoVersionElement, "versioningStyle", VersioningStyle.GetDefaultGlobalVariable());

                VersioningStyle.FromGlobalVariable(versioningStyle);
                StartDate = DateTime.Parse(GetProjectVariable(autoVersionElement, "startDate", "10/21/1975 00:00:00"), System.Globalization.CultureInfo.InvariantCulture);

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
        public void Save()
        {
            XDocument projectVersionDocument = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"));

            XElement autoVersionElement = new XElement("autoVersion");

            autoVersionElement.Add(new XAttribute("autoUpdateVersionData", AutoUpdateVersionData.ToString()));
            autoVersionElement.Add(new XAttribute("versioningStyle", VersioningStyle.ToGlobalVariable()));

            if (!string.IsNullOrEmpty(VersionFilename))
                autoVersionElement.Add(new XAttribute("versionFilename", VersionFilename));

            if (!string.IsNullOrEmpty(VersionFilePackage))
                autoVersionElement.Add(new XAttribute("versionFilePackage", VersionFilePackage));

            if (!string.IsNullOrEmpty(VersionTemplateFilename))
                autoVersionElement.Add(new XAttribute("versionTemplateFilename", VersionTemplateFilename));

            if (BuildAction != BuildActionType.Both)
                autoVersionElement.Add(new XAttribute("buildAction", BuildAction.ToString()));

            autoVersionElement.Add(new XAttribute("startDate", StartDate.ToString(System.Globalization.CultureInfo.InvariantCulture)));

            autoVersionElement.Add(new XAttribute("incrementBeforeBuild", IncrementBeforeBuild.ToString()));

            projectVersionDocument.Add(autoVersionElement);

            projectVersionDocument.Save(_versionFile);
        }

        #endregion
    }
}

