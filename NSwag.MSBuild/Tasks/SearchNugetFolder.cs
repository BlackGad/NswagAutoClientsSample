using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NSwag.MSBuild.Extensions;

namespace NSwag.MSBuild.Tasks
{
    public class SearchNugetFolder : Task
    {
        private readonly List<ITaskItem> _resultFolders;

        #region Constructors

        public SearchNugetFolder()
        {
            _resultFolders = new List<ITaskItem>();
        }

        #endregion

        #region Properties

        [Required]
        public string PackageID { get; set; }

        public string RelativeToPath { get; set; }

        [Output]
        public ITaskItem[] ResultFolders
        {
            get { return _resultFolders.ToArray(); }
        }

        public ITaskItem[] TargetFolders { get; set; }

        #endregion

        #region Override members

        public override bool Execute()
        {
            _resultFolders.Clear();
            try
            {
                Log.LogMessage(MessageImportance.Normal, "Searching Nuget package folder...");

                if (string.IsNullOrEmpty(PackageID)) throw new InvalidDataException("Nuget PackageID is not set");

                var targetFolders = (TargetFolders ?? Enumerable.Empty<ITaskItem>()).Select(f => f.ItemSpec).ToList();
                if (!targetFolders.Any())
                {
                    targetFolders.Add(Environment.CurrentDirectory);
                    targetFolders.Add(Environment.CurrentDirectory + "\\..\\");
                }

                var packageFolders = NugetExtensions.FindPackageLattestVersionFolders(PackageID, targetFolders.ToArray());

                if (!packageFolders.Any())
                {
                    Log.LogMessage(MessageImportance.Normal, "  * There is no packages found");
                    return true;
                }

                foreach (var packageFolder in packageFolders)
                {
                    var path = Path.GetFullPath(packageFolder.Path);
                    if (!string.IsNullOrEmpty(RelativeToPath)) path = IOExtensions.MakeRelativePath(RelativeToPath, path);
                    Log.LogMessage(MessageImportance.Normal, "  * Found package: {0}", path);
                    _resultFolders.Add(new TaskItem(path));
                }
            }
            catch (Exception e)
            {
                Log.LogError(e.ToString());
                return false;
            }

            return true;
        }

        #endregion
    }
}