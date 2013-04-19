using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Errordite.Core.Exceptions;

namespace Errordite.Core.FileSystem
{
    public static class RepositoryPathHelper
    {
        public static string ResolvePath(string relativePath)
        {
            ArgumentValidation.NotNull(relativePath);

            if (relativePath.StartsWith("$reproot"))
            {
                var hgRoot = FindParentDirectoryContainingDirectory(AppDomain.CurrentDomain.BaseDirectory, ".hg");

                if (hgRoot == null)
                    throw new ErrorditeRepositoryRootNotFoundException(AppDomain.CurrentDomain.BaseDirectory);

                relativePath = Path.Combine(hgRoot.FullName, relativePath.Replace(@"$reproot\", ""));

                return relativePath;
            }

            if (Path.IsPathRooted(relativePath))
                return relativePath;

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        }

        private static DirectoryInfo FindParentDirectoryContainingDirectory(string currentDir, string dirToFind)
        {
            return
                GetAncestors(new DirectoryInfo(currentDir)).FirstOrDefault(
                    d => Directory.GetDirectories(d.FullName, dirToFind).Any());
        }

        private static IEnumerable<DirectoryInfo> GetAncestors(DirectoryInfo dir)
        {
            yield return dir;

            while ((dir = Directory.GetParent(dir.FullName)) != null)
                yield return dir;
        }
    }
}
