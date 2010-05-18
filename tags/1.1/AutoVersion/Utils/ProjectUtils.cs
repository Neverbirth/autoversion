using PluginCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AutoVersion.Utils
{
    static class ProjectUtils
    {

        public static string GetClosestPath(string basePath)
        {
            string closest = GetClosestPath(basePath, PluginBase.CurrentProject.SourcePaths, false);
            
            if (closest == string.Empty)
            {
                IPlugin projectManager = PluginBase.MainForm.FindPlugin("30018864-fadd-1122-b2a5-779832cbbf23");

                if (projectManager != null)
                {
                    PropertyInfo prop = projectManager.Settings.GetType().GetProperty("GlobalClasspaths",
                                                                                      BindingFlags.Public |
                                                                                      BindingFlags.Instance |
                                                                                      BindingFlags.ExactBinding);
                    
                    closest = GetClosestPath(basePath, (List<string>)prop.GetValue(projectManager.Settings, null), true);
                }
            }

            return closest;
        }

        private static string GetClosestPath(string basePath, IEnumerable<string> pathCollection, bool absolutePaths)
        {
            string closest = string.Empty;
            string absolutePath;
            foreach (string classpath in pathCollection)
            {
                absolutePath = !absolutePaths ? PluginBase.CurrentProject.GetAbsolutePath(classpath) : classpath;
                if ((basePath.StartsWith(absolutePath) || absolutePath == ".") && absolutePath.Length > closest.Length)
                    closest = absolutePath;
            }
            
            return closest;
        }

        public static string GetBaseSourcePath()
        {
            string retVal = null;

            foreach (string path in PluginBase.CurrentProject.SourcePaths)
            {
                if (!System.IO.Path.IsPathRooted(path) && !path.StartsWith(".."))
                {
                    retVal = path;
                    break;
                }
            }

            if (retVal == null && PluginBase.CurrentProject.SourcePaths.Length > 0)
            {
                retVal = PluginBase.CurrentProject.SourcePaths[0];
            }

            return retVal;
        }
    }
}
