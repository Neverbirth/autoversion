﻿using System.Collections.Generic;
using PluginCore.Helpers;

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

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
            string retVal;

            if (string.IsNullOrEmpty(IncrementSettings.VersionTemplateFilename))
            {
                string fileName;

                string basePath = Path.Combine(PathHelper.TemplateDir, "AutoVersion");

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
                retVal = PluginCore.PluginBase.CurrentProject.GetAbsolutePath(IncrementSettings.VersionTemplateFilename);
            }

            return retVal;
        }

        private static string GetVersionArgRegexLine(IEnumerable<string> templateLines, string versionArg)
        {
            string dataLine = templateLines.FirstOrDefault(x => x.Contains(versionArg));

            if (string.IsNullOrEmpty(dataLine)) return "0";

            return LineToRegExPattern(dataLine, versionArg);
        }

        private static IEnumerable<string> GetVersionArgRegexLines(IEnumerable<string> templateLines, string versionArg)
        {
            return templateLines.Where(x => x.Contains(versionArg)).Select(x => LineToRegExPattern(x, versionArg));
        }

        public string GetVersionFilename()
        {
            string filename;

            if (!string.IsNullOrEmpty(IncrementSettings.VersionFilename))
            {
                filename = IncrementSettings.VersionFilename;
                return PluginCore.PluginBase.CurrentProject.GetAbsolutePath(filename);
            }
            else
            {
                string basePath = PluginCore.PluginBase.CurrentProject.GetAbsolutePath(
                    Utils.ProjectUtils.GetBaseSourcePath() ?? Path.GetDirectoryName(
                        PluginCore.PluginBase.CurrentProject.ProjectPath));
                filename = "Version." + (ProjectType == LanguageType.Haxe ? "hx" : "as");

                return Path.Combine(basePath, filename);
            }
        }

        private string GetVersionDataContent(string fileName)
        {
            string templateData = FileHelper.ReadFile(GetTemplateFileName());

            templateData = ParseTemplateData(templateData, fileName);

            return templateData;
        }

        private static string GetVersionDataValue(string versionData, string pattern)
        {
            if (Regex.IsMatch(versionData, pattern))
            {
                return
                    Regex.Match(versionData, pattern).Captures[0].Value;
            }
            
            return "0";
        }

        public bool IsAirProjector()
        {
            if (this.ProjectType == LanguageType.ActionScript2 || this.ProjectType == LanguageType.None)
                return false;

            XDocument projectDoc = XDocument.Load(PluginCore.PluginBase.CurrentProject.ProjectPath);

            if (this.ProjectType == LanguageType.ActionScript3)
            {
                return
                    (projectDoc.Element("project").Element("build").Elements("option").FirstOrDefault(
                        x => x.Attribute("additional") != null &&
                            x.Attribute("additional").Value.Split('\n').Contains("+configname=air")) != null);
            }
            
            return
                (projectDoc.Element("project").Element("haxelib").Elements("library").FirstOrDefault(x => x.Attribute("name").Value == "air") !=
                 null);
        }

        private static string LineToRegExPattern(string line, string arg)
        {
            string regExedArg = arg.Replace("$", "\\$").Replace("(", "\\(").Replace(")", "\\)");

            line = "(?<=" + line.Replace("\\", "\\\\").Replace("'", "\\'").Replace("}", "\\}").
                Replace(".", "\\.").Replace("+", "\\+").Replace("*", "\\*").Replace("/", "\\/").
                Replace("?", "\\?").Replace("^", "\\^").Replace("$", "\\$").Replace("|", "\\|").
                Replace("[", "\\[").Replace("]", "\\]").Replace("(", "\\(").Replace(")", "\\)").
                Replace("{", "\\{").Replace("#", "\\#").Replace(regExedArg, @")(\d+)(?=") + ")";

            line = Regex.Replace(line, @"\\\$\\\([A-Za-z]+\\\)", @"(?:.+|\r|\r\n)");

            return line;
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

                IEnumerable<string> templateLines = File.ReadAllLines(GetTemplateFileName()).Where(x => x.Contains("$(Major)") || x.Contains("$(Minor)") || x.Contains("$(Build)") || x.Contains("$(Revision)"));

                string major = GetVersionDataValue(fileVersionData, GetVersionArgRegexLine(templateLines, "$(Major)"));
                string minor = GetVersionDataValue(fileVersionData, GetVersionArgRegexLine(templateLines, "$(Minor)"));
                string build = GetVersionDataValue(fileVersionData, GetVersionArgRegexLine(templateLines, "$(Build)"));
                string revision = GetVersionDataValue(fileVersionData, GetVersionArgRegexLine(templateLines, "$(Revision)"));

                version = new Version(major + "." + minor + "." + build + "." + revision);
            }

            this.Version = version;
        }

        private string ParseTemplateData(string content, string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);

            // Process common args
            content = PluginCore.PluginBase.MainForm.ProcessArgString(content);

            // Process other args with needed info
            content = content.Replace("$(FileName)", fileName);

            if (content.Contains("$(FileNameWithPackage)") || content.Contains("$(Package)"))
            {
                string package = string.Empty;

                // Find closest parent
                string classpath = Utils.ProjectUtils.GetClosestPath(path);

                if (classpath != string.Empty)
                {
                    // Parse package name from path
                    package = Path.GetDirectoryName(Utils.IOUtils.MakeRelativePath(classpath + Path.DirectorySeparatorChar, path));
                    package = package.Replace(Path.DirectorySeparatorChar, '.');
                }

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
                content = GetVersionDataContent(versionFile);
            }
            else
            {
                content = FileHelper.ReadFile(versionFile);

                IEnumerable<string> templateLines = File.ReadAllLines(GetTemplateFileName()).Where(x => x.Contains("$(Major)") || x.Contains("$(Minor)") || x.Contains("$(Build)") || x.Contains("$(Revision)"));

                SetVersionDataValue(ref content, GetVersionArgRegexLines(templateLines, "$(Major)"), Version.Major);
                SetVersionDataValue(ref content, GetVersionArgRegexLines(templateLines, "$(Minor)"), Version.Minor);
                SetVersionDataValue(ref content, GetVersionArgRegexLines(templateLines, "$(Build)"), Version.Build);
                SetVersionDataValue(ref content, GetVersionArgRegexLines(templateLines, "$(Revision)"), Version.Revision);
            }

            FileHelper.WriteFile(versionFile, content,
                                 encoding, PluginCore.PluginBase.Settings.SaveUnicodeWithBOM);
        }

        private static void SetVersionDataValue(ref string versionData, IEnumerable<string> patterns, int newValue)
        {
            foreach (string pattern in patterns)
                versionData = Regex.Replace(versionData, pattern, newValue.ToString());
        }

        public void UpdateAirVersion()
        {
            string airPropertiesDescriptor;

            if (IncrementSettings.AirDescriptorFile == string.Empty)
                airPropertiesDescriptor = Path.Combine(Path.GetDirectoryName(PluginCore.PluginBase.CurrentProject.ProjectPath), "application.xml");
            else
                airPropertiesDescriptor = PluginCore.PluginBase.CurrentProject.GetAbsolutePath(IncrementSettings.AirDescriptorFile);

            if (!File.Exists(airPropertiesDescriptor)) return;

            XNamespace airNs = "http://ns.adobe.com/air/application/1.5";

            XDocument airPropertiesDoc = XDocument.Load(airPropertiesDescriptor);

            XElement versionElement = airPropertiesDoc.Element(airNs + "application").Element(airNs + "version");

            if (versionElement != null)
            {
                versionElement.SetValue(this.Version.ToString());
            }
            else
            {
                versionElement = new XElement(airNs + "version", this.Version.ToString());
                airPropertiesDoc.Element(airNs + "application").Add(versionElement);
            }

            airPropertiesDoc.Save(airPropertiesDescriptor);
        }

        #endregion

    }
}