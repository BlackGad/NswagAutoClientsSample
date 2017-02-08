namespace NSwag.MSBuild.Extensions
{
    public static class IntegerExtensions
    {
        #region Static members

        public static int MergeHash(this int hash, int addHash)
        {
            return (hash*397) ^ addHash;
        }

        #endregion
    }
}