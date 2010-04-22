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
        /// Gets or sets the assembly info filename.
        /// </summary>
        /// <value>The assembly info filename.</value>
        [Category("Increment Settings")]
        [Description("Use this value if the version attributes aren't saved in the default file. ")]
        [DefaultValue("")]
        [DisplayName("Version Filename")]
        [EditorAttribute(typeof(FileNameEditor), typeof(UITypeEditor))]
        public string VersionFilename
        {
            get { return _versionFilename; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    // Make the path relative

                    //                    string basePath = Path.GetDirectoryName(SolutionItem.Filename);
                    //                    _assemblyInfoFilename = Common.MakeRelativePath(basePath, value);
                }
                else
                    _versionFilename = string.Empty;
            }
        }

        private bool _autoUpdateVersionData;
        /// <summary>
        /// Gets or sets a value indicating whether to auto update the file version.
        /// </summary>
        /// <value>
        /// 	<c>true</c> to auto update the file version; otherwise, <c>false</c>.
        /// </value>
        [Category("Increment Settings")]
        [Description("Auto update the version data.")]
        [DisplayName("Update version data")]
        [DefaultValue(false)]
        public bool AutoUpdateVersionData
        {
            get { return _autoUpdateVersionData; }
            set { _autoUpdateVersionData = value; }
        }

        private DateTime _projectStartDate;
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>The start date.</value>
        [Category("Increment Settings")]
        [Description("The start date to use.")]
        [DisplayName("Start Date")]
        [DefaultValue(typeof(DateTime), "1975/10/21")]
        public DateTime StartDate
        {
            get { return _projectStartDate; }
            set { _projectStartDate = value; }
        }

        private bool _isUniversalTime;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is using UTC.
        /// </summary>
        /// <value><c>true</c> if this instance is using UTC; otherwise, <c>false</c>.</value>
        [Category("Increment Settings")]
        [Description("Indicates wheter to use Coordinated Universal Time (UTC) time stamps.")]
        [DisplayName("Use Coordinated Universal Time")]
        [DefaultValue(false)]
        public bool IsUniversalTime
        {
            get { return this._isUniversalTime; }
            set { this._isUniversalTime = value; }
        }

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

        private BuildActionType _buildAction;
        /// <summary>
        /// Gets or sets the build action
        /// </summary>
        /// <value>The build action on which the auto update should occur.</value>
        [Category("Condition")]
        [DefaultValue(BuildActionType.Both)]
        [DisplayName("Build Action")]
        [Description("Set this to the desired build action when the auto update should occur.")]
        public BuildActionType BuildAction
        {
            get { return _buildAction; }
            set { _buildAction = value; }
        }

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
                if (File.Exists(_versionFile))
                {
                    XDocument projectVersionDocument = XDocument.Load(_versionFile);
                    XElement autoVersionElement = projectVersionDocument.Element("autoVersion");

                    AutoUpdateVersionData = bool.Parse(GetProjectVariable(autoVersionElement, "autoUpdateVersionData", "false"));
                    VersionFilename = GetProjectVariable(autoVersionElement, "versionFileName", string.Empty);

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
                    //StartDate = DateTime.Parse(GlobalVariables.GetGlobalVariable(SolutionItem.Globals, Resources.GlobalVarName_startDate, "1975/10/21"));*/
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
        public void Save()
        {
            XDocument projectVersionDocument = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"));

            XElement autoVersionElement = new XElement("autoVersion");

            autoVersionElement.Add(new XAttribute("autoUpdateVersionData", AutoUpdateVersionData.ToString()));
            autoVersionElement.Add(new XAttribute("versioningStyle", VersioningStyle.ToGlobalVariable()));

            if (!string.IsNullOrEmpty(VersionFilename))
            {
                autoVersionElement.Add(new XAttribute("versionFilename", VersionFilename));
            }

            if (BuildAction != BuildActionType.Both)
            {
                autoVersionElement.Add(new XAttribute("buildAction", BuildAction.ToString()));
            }

            projectVersionDocument.Add(autoVersionElement);

            projectVersionDocument.Save(_versionFile);
        }

        #endregion
    }
}

