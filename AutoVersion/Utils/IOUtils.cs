using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AutoVersion.Utils
{
    internal static class IOUtils
    {

        public static string MakeAbsolutePath(string basePath, string relativePath)
        {
            if (string.IsNullOrEmpty(basePath))
                throw new ArgumentNullException("basePath");

            if (string.IsNullOrEmpty(relativePath))
                throw new ArgumentNullException("relativePath");

            if (Path.IsPathRooted(relativePath)) return relativePath;

            if (!Path.IsPathRooted(basePath))
                throw new ArgumentException("Base path cannot be a relative uri.");

            return new Uri(new Uri(basePath), relativePath).LocalPath;
        }

        public static string MakeRelativePath(string basePath, string targetPath)
        {
            if (string.IsNullOrEmpty(basePath))
                throw new ArgumentNullException("basePath");

            if (string.IsNullOrEmpty(targetPath))
                throw new ArgumentNullException("targetPath");

            bool isRooted = Path.IsPathRooted(basePath) && Path.IsPathRooted(targetPath);

            if (isRooted)
            {
                bool isDifferentRoot = string.Compare(Path.GetPathRoot(basePath),
                                                      Path.GetPathRoot(targetPath), true) != 0;

                if (isDifferentRoot)
                    return targetPath;
            }
            else
            {
                return targetPath;
            }

            Uri basePathUri = new Uri(basePath);
            Uri targetPathUri = new Uri(targetPath);

            return basePathUri.MakeRelativeUri(targetPathUri).ToString().Replace('/', Path.DirectorySeparatorChar);
        }
    }
}

