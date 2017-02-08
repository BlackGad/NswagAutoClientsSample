using System.IO;

namespace NSwag.MSBuild.Extensions
{
    public static class DirectoryExtensions
    {
        #region Static members

        public static string EnsureDirectoryExist(this string directory)
        {
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            return directory;
        }

        #endregion
    }
}