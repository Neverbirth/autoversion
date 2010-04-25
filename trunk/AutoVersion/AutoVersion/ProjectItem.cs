using PluginCore.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AutoVersion
{
    /// <summary>
    /// The language type of the project
    /// </summary>
    internal enum LanguageType
    {
        /// <summary>
        /// This is no work project
        /// </summary>
        None,
        /// <summary>
        /// ActionScript 3
        /// </summary>
        ActionScript3
    }

    /// <summary>
    /// Class to wrap project items
    /// </summary>
    class ProjectItem
    {

        #region  Properties

        public Version Version { get; set; }
        public VersioningSettings IncrementSettings { get; private set; }
        public LanguageType ProjectType { get; set; }

        #endregion

        #region  Constructor

        public ProjectItem()
        {
            this.IncrementSettings = new VersioningSettings();
            this.IncrementSettings.Load();
            this.ProjectType = LanguageType.ActionScript3;
            this.LoadVersion();
        }

        #endregion

        #region  Methods

        public string GetVersionFilename()
        {
            string basePath = Path.GetDirectoryName(PluginCore.PluginBase.CurrentProject.ProjectPath);
            string filename = "Version.as";

            if (!string.IsNullOrEmpty(IncrementSettings.VersionFilename))
            {
                filename = IncrementSettings.VersionFilename;
                return Utils.IOUtils.MakeAbsolutePath(basePath + Path.DirectorySeparatorChar, filename);
            }

            return Path.Combine(basePath, filename);
        }

        private string GetVersionDataContent(string fileName)
        {
            if (string.IsNullOrEmpty(IncrementSettings.VersionTemplateFilename))
            {
                return ParseTemplateData(Properties.Resources.DefaultVersioningTemplate, fileName);
            }
            else
            {
                string basePath = Path.GetDirectoryName(PluginCore.PluginBase.CurrentProject.ProjectPath);
                string templatePath = Utils.IOUtils.MakeAbsolutePath(basePath + Path.DirectorySeparatorChar, IncrementSettings.VersionTemplateFilename);
                string templateData = FileHelper.ReadFile(templatePath);

                templateData = ParseTemplateData(templateData, fileName);

                return templateData;
            }
        }

        private string GetVersionDataValue(string versionData, string pattern)
        {
            try
            {
                return
                    Regex.Matches(versionData, pattern)[0].Groups[1].Captures[0].Value;
            }
            catch (Exception ex)
            {
                return "0";
            }
        }

        public void LoadVersion()
        {
            Version version;
            string versionFile = GetVersionFilename();

            if (!File.Exists(versionFile))
            {
                version = new Version(1, 0, 0, 0);
            }
            else
            {
                string fileVersionData = FileHelper.ReadFile(versionFile);

                if (string.IsNullOrEmpty(IncrementSettings.VersionTemplateFilename))
                {
                    string major = GetVersionDataValue(fileVersionData, @"static public const Major:int = (\d+);");
                    string minor = GetVersionDataValue(fileVersionData, @"static public const Minor:int = (\d+);");
                    string build = GetVersionDataValue(fileVersionData, @"static public const Build:int = (\d+);");
                    string revision = GetVersionDataValue(fileVersionData, @"static public const Revision:int = (\d+);");

                    version = new Version(major + "." + minor + "." + build + "." + revision);
                }
                else
                {
                    version = new Version(1, 0, 0, 0);
                }
            }

            this.Version = version;
        }

        private string ParseTemplateData(string content, string fileName)
        {
            // Process common args
            content = PluginCore.PluginBase.MainForm.ProcessArgString(content);

            // Process other args with needed info
            content = content.Replace("$(FileName)", fileName);

            if (content.Contains("$(FileNameWithPackage)") || content.Contains("$(Package)"))
            {
                string package = IncrementSettings.VersionFilePackage;

                content = content.Replace("$(Package)", package);

                if (package != "")
                    content = content.Replace("$(FileNameWithPackage)", package + "." + fileName);
                else
                    content = content.Replace("$(FileNameWithPackage)", fileName);
            }

            // Process action points just in case...
            content = content.Replace(SnippetHelper.BOUNDARY, string.Empty).
                Replace(SnippetHelper.ENTRYPOINT, string.Empty).
                Replace(SnippetHelper.EXITPOINT, string.Empty);

            // Process AutoVersion args
            content = content.Replace("$(Major)", Version.Major.ToString()).
                Replace("$(Minor)", Version.Minor.ToString()).
                Replace("$(Build)", Version.Build.ToString()).
                Replace("$(Revision)", Version.Revision.ToString());

            return content;
        }

        public void SaveVersion()
        {
            string versionFile = GetVersionFilename();
            Encoding encoding = Encoding.GetEncoding((Int32)PluginCore.PluginBase.Settings.DefaultCodePage);

            FileHelper.WriteFile(versionFile, GetVersionDataContent(Path.GetFileNameWithoutExtension(versionFile)), 
                encoding, PluginCore.PluginBase.Settings.SaveUnicodeWithBOM);
        }

        #endregion
    }
}
