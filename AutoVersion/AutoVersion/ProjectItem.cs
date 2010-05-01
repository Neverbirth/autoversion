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
        /// ActionScript 2
        /// </summary>
        ActionScript2,
        /// <summary>
        /// ActionScript 3
        /// </summary>
        ActionScript3,
        /// <summary>
        /// haXe
        /// </summary>
        Haxe
    }

    /// <summary>
    /// Class to wrap project items
    /// </summary>
    class ProjectItem
    {

        #region  Properties

        public Version Version { get; set; }
        public ProjectItemIncrementSettings IncrementSettings { get; private set; }
        public LanguageType ProjectType { get; set; }

        #endregion

        #region  Constructor

        public ProjectItem()
        {
            this.IncrementSettings = new ProjectItemIncrementSettings();
            this.IncrementSettings.Load();

            switch (PluginCore.PluginBase.CurrentProject.Language)
            {
                case "as2":
                    this.ProjectType = LanguageType.ActionScript2;
                    break;
                case "haxe":
                    this.ProjectType = LanguageType.Haxe;
                    break;
                default:
                    this.ProjectType = LanguageType.ActionScript3;
                    break;
            }

            this.LoadVersion();
        }

        #endregion

        #region  Methods

        public void ApplyGlobalSettings()
        {
            IncrementSettings.CopyFrom(GlobalIncrementSettings.GetInstance());
        }

        private string GetTemplateFileName()
        {
            string basePath, retVal;

            if (string.IsNullOrEmpty(IncrementSettings.VersionTemplateFilename))
            {
                string fileName;

                basePath = Path.Combine(PathHelper.TemplateDir, "AutoVersion");

                switch (ProjectType)
                {
                    case LanguageType.ActionScript2:
                        fileName = "ActionScript 2.fdt";
                        break;
                    case LanguageType.Haxe:
                        fileName = "haXe.fdt";
                        break;
                    default:
                        fileName = "ActionScript 3.fdt";
                        break;
                }

                retVal = Path.Combine(basePath, fileName);
            }
            else
            {
                basePath = Path.GetDirectoryName(PluginCore.PluginBase.CurrentProject.ProjectPath);
                retVal = Utils.IOUtils.MakeAbsolutePath(basePath + Path.DirectorySeparatorChar, IncrementSettings.VersionTemplateFilename);
            }

            return retVal;
        }

        private string GetVersionArgRegexLine(string[] templateLines, string versionArg)
        {
            string dataLine = templateLines.FirstOrDefault((x) => x.Contains(versionArg));

            if (string.IsNullOrEmpty(dataLine)) return "0";

            string regExedArg = versionArg.Replace("$", "\\$").Replace("(", "\\(").Replace(")", "\\)");

            dataLine = "(?<=" + dataLine.Replace("\\", "\\\\").Replace("'", "\\'").Replace("}", "\\}").
                Replace(".", "\\.").Replace("+", "\\+").Replace("*", "\\*").Replace("/", "\\/").
                Replace("?", "\\?").Replace("^", "\\^").Replace("$", "\\$").Replace("|", "\\|").
                Replace("[", "\\[").Replace("]", "\\]").Replace("(", "\\(").Replace(")", "\\)").
                Replace("{", "\\{").Replace("#", "\\#").Replace(regExedArg, @")(\d+)(?=") + ")";

            dataLine = Regex.Replace(dataLine, @"\\\$\\\([A-Za-z]+\\\)", @"(?:.+|\r|\r\n)");

            return dataLine;
        }

        public string GetVersionFilename()
        {
            string basePath = Path.GetDirectoryName(PluginCore.PluginBase.CurrentProject.ProjectPath);
            string filename = "Version." + (ProjectType == LanguageType.Haxe ? "hx" : "as");
            
            if (!string.IsNullOrEmpty(IncrementSettings.VersionFilename))
            {
                filename = IncrementSettings.VersionFilename;
                return Utils.IOUtils.MakeAbsolutePath(basePath + Path.DirectorySeparatorChar, filename);
            }

            return Path.Combine(basePath, filename);
        }

        private string GetVersionDataContent(string fileName)
        {
            string templateData = FileHelper.ReadFile(GetTemplateFileName());

            templateData = ParseTemplateData(templateData, fileName);

            return templateData;
        }

        private string GetVersionDataValue(string versionData, string pattern)
        {
            if (Regex.IsMatch(versionData, pattern))
            {
                return
                    Regex.Match(versionData, pattern).Captures[0].Value;
            }
            else
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

                string[] templateLines = File.ReadAllLines(GetTemplateFileName());

                string major = GetVersionDataValue(fileVersionData, GetVersionArgRegexLine(templateLines, "$(Major)"));
                string minor = GetVersionDataValue(fileVersionData, GetVersionArgRegexLine(templateLines, "$(Minor)"));
                string build = GetVersionDataValue(fileVersionData, GetVersionArgRegexLine(templateLines, "$(Build)"));
                string revision = GetVersionDataValue(fileVersionData, GetVersionArgRegexLine(templateLines, "$(Revision)"));

                version = new Version(major + "." + minor + "." + build + "." + revision);
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
            string content;
            Encoding encoding = Encoding.GetEncoding((Int32)PluginCore.PluginBase.Settings.DefaultCodePage);

            if (!IncrementSettings.SmartUpdate || !File.Exists(versionFile))
            {
                content = GetVersionDataContent(Path.GetFileNameWithoutExtension(versionFile));
            }
            else
            {
                content = FileHelper.ReadFile(versionFile);

                string[] templateLines = File.ReadAllLines(GetTemplateFileName());

                SetVersionDataValue(ref content, GetVersionArgRegexLine(templateLines, "$(Major)"), Version.Major);
                SetVersionDataValue(ref content, GetVersionArgRegexLine(templateLines, "$(Minor)"), Version.Minor);
                SetVersionDataValue(ref content, GetVersionArgRegexLine(templateLines, "$(Build)"), Version.Build);
                SetVersionDataValue(ref content, GetVersionArgRegexLine(templateLines, "$(Revision)"), Version.Revision);
            }

            FileHelper.WriteFile(versionFile, content,
                                 encoding, PluginCore.PluginBase.Settings.SaveUnicodeWithBOM);
        }

        private void SetVersionDataValue(ref string versionData, string pattern, int newValue)
        {
            versionData = Regex.Replace(versionData, pattern, newValue.ToString());
        }

        #endregion

    }
}
