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
                string fileVersionData = File.ReadAllText(versionFile);

                string major = GetVersionDataValue(fileVersionData, @"static public const Major:int = (\d+);");
                string minor = GetVersionDataValue(fileVersionData, @"static public const Minor:int = (\d+);");
                string build = GetVersionDataValue(fileVersionData, @"static public const Build:int = (\d+);");
                string revision = GetVersionDataValue(fileVersionData, @"static public const Revision:int = (\d+);");
                
                version = new Version(major + "." + minor + "." + build + "." + revision);
            }

            this.Version = version;
        }

        public void SaveVersion()
        {
            string versionFile = GetVersionFilename();
            
            using (FileStream fileStream = new FileStream(versionFile, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(fileStream, new UTF8Encoding(false)))
                {
                    writer.WriteLine("package " + IncrementSettings.VersionFilePackage);
                    writer.WriteLine("{");
                    writer.WriteLine("	public final class Version");
                    writer.WriteLine("	{");
                    writer.WriteLine("		static public const Major:int = " + Version.Major.ToString() + ";");
                    writer.WriteLine("		static public const Minor:int = " + Version.Minor.ToString() + ";");
                    writer.WriteLine("		static public const Build:int = " + Version.Build.ToString() + ";");
                    writer.WriteLine("		static public const Revision:int = " + Version.Revision.ToString() + ";");
                    writer.WriteLine("	}");
                    writer.WriteLine("}");
                }
            }
        }

        #endregion
    }
}
