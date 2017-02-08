using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NSwag.MSBuild.Sources;

namespace NSwag.MSBuild.Extensions
{
    public static class NugetExtensions
    {
        #region Static members

        public static List<PackageFolder> FindPackageLattestVersionFolders(string packageID, params string[] targetFolders)
        {
            var result = new List<PackageFolder>();
            var possibleNugetFolders = new List<string>
            {
                "_Toolkits",
                "packages",
                "."
            };

            foreach (var solutionFolder in targetFolders)
            {
                foreach (var folder in possibleNugetFolders)
                {
                    var rootDirectory = Path.Combine(solutionFolder, folder);
                    if (!Directory.Exists(rootDirectory)) continue;

                    var pair = GetLattestVersionFolder(packageID, rootDirectory);
                    if (pair == null) continue;

                    var packageVersion = pair.Value.Key;
                    var packagePath = pair.Value.Value;

                    result.Add(new PackageFolder
                    {
                        Version = packageVersion,
                        Path = packagePath
                    });
                }
            }

            return result;
        }

        private static KeyValuePair<Version, string>? GetLattestVersionFolder(string packageId, string directoryName)
        {
            if (directoryName == null) throw new ArgumentException("Illegal archive directory");

            var regexMask = new Regex((packageId + ".*").Replace(".", "\\.").Replace("*", ".*"), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            var regexVersion =
                new Regex(@"(?<id>^.+?(?=[\._]\d+[\._]\d+([\._]\d+[\._]\d+)?))[\._](?<major>\d+)[\._](?<minor>\d+)([\._](?<build>\d+)([\._](?<revision>\d+))?)?");

            var files = new ConcurrentDictionary<Version, string>();
            foreach (var directory in Directory.EnumerateDirectories(directoryName))
            {
                if (!regexMask.IsMatch(directory)) continue;

                var versionMatch = regexVersion.Match(directory);

                var id = versionMatch.Groups["id"].ToString();
                if (!id.EndsWith(packageId, StringComparison.InvariantCultureIgnoreCase)) continue;
                var major = versionMatch.Groups["major"].ToString();
                var minor = versionMatch.Groups["minor"].ToString();
                var build = versionMatch.Groups["build"].ToString();
                var revision = versionMatch.Groups["revision"].ToString();
                major = string.IsNullOrEmpty(major) ? "0" : major;
                minor = string.IsNullOrEmpty(minor) ? "0" : minor;
                build = string.IsNullOrEmpty(build) ? "0" : build;
                revision = string.IsNullOrEmpty(revision) ? "0" : revision;

                var versionString = string.Format("{0}.{1}.{2}.{3}", major, minor, build, revision);

                files.TryAdd(new Version(versionString), directory);
            }
            return files.IsEmpty
                ? (KeyValuePair<Version, string>?)null
                : files.OrderByDescending(f => f.Key).FirstOrDefault();
        }

        #endregion
    }
}