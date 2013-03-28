
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Errordite.Utils.DevUtility.Entities;

namespace Errordite.Utils.DevUtility
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string GetRepositoryRoot(string dir = null)
        {
            dir = dir ?? AppDomain.CurrentDomain.BaseDirectory;

            if (Directory.GetDirectories(dir, ".hg").Any())
                return dir;

            if (Directory.GetParent(dir) == null)
                return null;

            return GetRepositoryRoot(Directory.GetParent(dir).FullName);
        }

        public static IList<Repository> GetRepositories()
        {
            var repoRoot = App.GetRepositoryRoot();

            if (repoRoot == null)
                return new Repository[0];

            return Directory.GetDirectories(Directory.GetParent(repoRoot).FullName).Where(
                d => Directory.GetDirectories(d, ".hg").Any()).Select(
                    d =>
                        {
                            var r = new Repository
                            {
                                LocalLocation = d,
                                ParentRepo = GetParentRepo(d),
                                ContainsThisApp = repoRoot == d,
                                Name = Regex.Replace(new DirectoryInfo(d).Name, "[^a-z|^A-Z|^0-9]", ""),
                            };
                            return r;
                        }).ToList();
        }

        public static string GetParentRepo(string repo)
        {
            string hgrcFile = Path.Combine(repo, @".hg\hgrc");
            if (!File.Exists(hgrcFile))
                return null;

            using (var sr = new StreamReader(hgrcFile))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                    if (line.StartsWith("Default", StringComparison.InvariantCultureIgnoreCase))
                        return line;
            }

            return null;
        }
    }
}
