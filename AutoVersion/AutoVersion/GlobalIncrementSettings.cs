using PluginCore.Helpers;
using PluginCore.Utilities;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace AutoVersion
{
    class GlobalIncrementSettings : BaseIncrementSettings
    {

        #region  Enums

        /// <summary>
        /// When the Global Settings should be applied to projects.
        /// </summary>
        public enum ApplyGlobalSettings
        {
            /// <summary>
            /// Only if explicit chosen to use them
            /// </summary>
            OnlyWhenChosen,
            /// <summary>
            /// Trigger the UseGlobalSettings to true for new projects
            /// </summary>
            AsDefault,
            /// <summary>
            /// Override the UseGlobalSettings flag to true whatever happens.
            /// I'll show some warnings if that one is active.
            /// </summary>
            Always
        }

        #endregion

        #region  Fields

        private static volatile GlobalIncrementSettings _instance = new GlobalIncrementSettings();

        private string _globalSettingsDir;
        private string _globalSettingsFile;

        #endregion

        #region  Properties

        private ApplyGlobalSettings _apply;
        /// <summary>
        /// Gets or Sets the setting when to apply the Global Settings.
        /// </summary>
        [Browsable(true)]
        [Category("Global")]
        [DisplayName("Apply Global Settings")]
        [Description("The setting when to use the Global Settings")]
        [DefaultValue(typeof(ApplyGlobalSettings), "OnlyWhenChosen")]
        public ApplyGlobalSettings Apply
        {
            get { return _apply; }
            set { _apply = value; }
        }

        #endregion

        #region  Constructor

        private GlobalIncrementSettings()
        {
            _globalSettingsDir = Path.Combine(PathHelper.DataDir, "AutoVersion");
            _globalSettingsFile = Path.Combine(_globalSettingsDir, "Settings.fdb");

            if (!Directory.Exists(_globalSettingsDir))
                Directory.CreateDirectory(_globalSettingsDir);

            Load();
        }

        #endregion

        #region  Methods

        public static GlobalIncrementSettings GetInstance()
        {
            return _instance;
        }

        public override void Load()
        {
            try
            {
                XDocument projectVersionDocument;

                if (File.Exists(_globalSettingsFile))
                    projectVersionDocument = XDocument.Load(_globalSettingsFile);
                else
                    projectVersionDocument = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"),
                        new XElement("autoVersion"));

                XElement autoVersionElement = projectVersionDocument.Element("autoVersion");

                Apply = (ApplyGlobalSettings)Enum.Parse(typeof(ApplyGlobalSettings), autoVersionElement.GetAttributeValue("apply", "OnlyWhenChosen"));
                AutoUpdateVersionData = bool.Parse(autoVersionElement.GetAttributeValue("autoUpdateVersionData", "false"));
                IncrementBeforeBuild = bool.Parse(autoVersionElement.GetAttributeValue("incrementBeforeBuild", "true"));

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

            }
            catch (Exception ex)
            {
                throw;
                //Logger.Write("Error occured while reading BuildVersionIncrement settings from \"" + SolutionItem.Filename + "\"\n" + ex.ToString(), LogLevel.Error);
            }
        }

        public override void Save()
        {
            XDocument projectVersionDocument = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"));

            XElement autoVersionElement = new XElement("autoVersion");

            autoVersionElement.Add(new XAttribute("apply", Apply.ToString()));
            autoVersionElement.Add(new XAttribute("autoUpdateVersionData", AutoUpdateVersionData.ToString()));
            autoVersionElement.Add(new XAttribute("versioningStyle", VersioningStyle.ToGlobalVariable()));

            if (BuildAction != BuildActionType.Both)
                autoVersionElement.Add(new XAttribute("buildAction", BuildAction.ToString()));

            autoVersionElement.Add(new XAttribute("startDate", StartDate.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            autoVersionElement.Add(new XAttribute("incrementBeforeBuild", IncrementBeforeBuild.ToString()));

            projectVersionDocument.Add(autoVersionElement);

            projectVersionDocument.Save(_globalSettingsFile);
        }

        #endregion

    }
}
