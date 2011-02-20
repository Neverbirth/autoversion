using PluginCore;
using PluginCore.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
#if NET_35
using System.Linq;
using System.Xml.Linq;
#else
using System.Xml;
#endif

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

        #region Constants

        /// <summary>
        /// Regex for tab replacing
        /// </summary>
        private readonly static Regex reTabs = new Regex("^\\t+", RegexOptions.Multiline | RegexOptions.Compiled);

        #endregion

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

            switch (PluginBase.CurrentProject.Language)
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

        public string GetTemplateFileName()
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
                retVal = PluginBase.CurrentProject.GetAbsolutePath(IncrementSettings.VersionTemplateFilename);
            }

            return retVal;
        }

        private static string GetVersionArgRegexLine(IEnumerable<string> templateLines, string versionArg)
        {
#if NET_35

            string dataLine = templateLines.FirstOrDefault(x => x.Contains(versionArg));

#else

            string dataLine = null;

            foreach (string line in templateLines)
            {
                if (line.Contains(versionArg))
                {
                    dataLine = line;
                    break;
                }
            }

#endif

            if (string.IsNullOrEmpty(dataLine)) return "0";

            return LineToRegExPattern(dataLine, versionArg);
        }

        private static IEnumerable<string> GetVersionArgRegexLines(IEnumerable<string> templateLines, string versionArg)
        {
#if NET_35
            return templateLines.Where(x => x.Contains(versionArg)).Select(x => LineToRegExPattern(x, versionArg));
#else
            foreach (string line in templateLines)
            {
                if (line.Contains(versionArg))
                {
                    yield return LineToRegExPattern(line, versionArg);
                }
            }
#endif
        }

        public string GetVersionFilename()
        {
            string filename;

            if (!string.IsNullOrEmpty(IncrementSettings.VersionFilename))
            {
                filename = IncrementSettings.VersionFilename;
                return PluginBase.CurrentProject.GetAbsolutePath(filename);
            }
            else
            {
                string basePath = PluginBase.CurrentProject.GetAbsolutePath(
                    Utils.ProjectUtils.GetBaseSourcePath() ?? Path.GetDirectoryName(
                        PluginBase.CurrentProject.ProjectPath));
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

            string[] compilerOptions;

            if (this.ProjectType == LanguageType.ActionScript3)
            {
                compilerOptions = ((ProjectManager.Projects.AS3.AS3Project)PluginBase.CurrentProject).CompilerOptions.Additional;
                foreach (string compilerOption in compilerOptions)
                {
                    if (compilerOption.Contains("configname=air"))
                    {
                        return true;
                    }
                }

                return false;
            }

            compilerOptions = ((ProjectManager.Projects.Haxe.HaxeProject)PluginBase.CurrentProject).CompilerOptions.Libraries;

            foreach (string name in compilerOptions)
            {
                if (name == "air")
                {
                    return true;
                }
            }

            return false;
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

#if NET_35
                IEnumerable<string> templateLines = File.ReadAllLines(GetTemplateFileName()).
                    Where(x => x.Contains("$(Major)") || x.Contains("$(Minor)") || x.Contains("$(Build)") || x.Contains("$(Revision)")).
                    Select(x => !PluginBase.Settings.UseTabs ? reTabs.Replace(x, new MathEvaluator(ReplaceTabs)) : x);
#else
                List<string> templateLines = new List<string>();
                string[] templateFileLines = File.ReadAllLines(GetTemplateFileName());
                string templateLine;

                for (int i = 0, lineCount = templateFileLines.Length; i < lineCount; i++)
                {
                    templateLine = templateFileLines[i];

                    if (templateLine.Contains("$(Major)") || templateLine.Contains("$(Minor)") || templateLine.Contains("$(Build)") || templateLine.Contains("$(Revision)"))
                    {
                        if (!PluginBase.Settings.UseTabs) templateLine = reTabs.Replace(templateLine, new MatchEvaluator(ReplaceTabs));
                        
                        templateLines.Add(templateLine);
                    }

                }

#endif

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

                if (package != String.Empty)
                    content = content.Replace("$(FileNameWithPackage)", package + "." + fileName);
                else
                    content = content.Replace("$(FileNameWithPackage)", fileName);
            }

            // Process common args
            content = PluginBase.MainForm.ProcessArgString(content);

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

        /// <summary>
        /// Match evaluator for tabs
        /// </summary>
        private static String ReplaceTabs(Match match)
        {
            return new String(' ', match.Length * PluginBase.Settings.IndentSize);
        }

        public void SaveVersion()
        {
            string versionFile = GetVersionFilename();
            string content;
            Encoding encoding = Encoding.GetEncoding((Int32)PluginBase.Settings.DefaultCodePage);

            if (!IncrementSettings.SmartUpdate || !File.Exists(versionFile))
            {
                content = GetVersionDataContent(versionFile);
            }
            else
            {
                content = FileHelper.ReadFile(versionFile);

#if NET_35

                IEnumerable<string> templateLines = File.ReadAllLines(GetTemplateFileName()).Where(x => x.Contains("$(Major)") || x.Contains("$(Minor)") || x.Contains("$(Build)") || x.Contains("$(Revision)"));

#else

                List<string> templateLines = new List<string>();
                string[] templateFileLines = File.ReadAllLines(GetTemplateFileName());
                string line;

                for (int i = 0, templateLinesCount = templateFileLines.Length; i < templateLinesCount; i++)
                {
                    line = templateFileLines[i];

                    if (line.Contains("$(Major)") || line.Contains("$(Minor)") || line.Contains("$(Build)") || line.Contains("$(Revision)"))
                    {
                        templateLines.Add(line);
                    }
                }


#endif

                SetVersionDataValue(ref content, GetVersionArgRegexLines(templateLines, "$(Major)"), Version.Major);
                SetVersionDataValue(ref content, GetVersionArgRegexLines(templateLines, "$(Minor)"), Version.Minor);
                SetVersionDataValue(ref content, GetVersionArgRegexLines(templateLines, "$(Build)"), Version.Build);
                SetVersionDataValue(ref content, GetVersionArgRegexLines(templateLines, "$(Revision)"), Version.Revision);
            }

            FileHelper.WriteFile(versionFile, content, encoding, PluginBase.Settings.SaveUnicodeWithBOM);
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
                airPropertiesDescriptor = Path.Combine(Path.GetDirectoryName(PluginBase.CurrentProject.ProjectPath), "application.xml");
            else
                airPropertiesDescriptor = PluginBase.CurrentProject.GetAbsolutePath(IncrementSettings.AirDescriptorFile);

            if (!File.Exists(airPropertiesDescriptor)) return;

#if NET_35

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

#else

            XmlDocument document = new XmlDocument();
            XmlNodeList versionNodes;

            try
            {
                document.Load(airPropertiesDescriptor);
                versionNodes = document.GetElementsByTagName("version", "http://ns.adobe.com/air/application/1.5");

                if (versionNodes.Count == 0)
                {
                    XmlNode versionNode = document.CreateElement("version");
                    versionNode.InnerText = Version.ToString();

                    document.DocumentElement.AppendChild(versionNode);
                }
                else
                {
                    versionNodes[0].InnerText = Version.ToString();
                }

                document.Save(airPropertiesDescriptor);
            }
            catch (Exception ex)
            {
                return;
            }


#endif

        }

        #endregion

    }
}
