using System;

namespace NSwag.MSBuild.Sources
{
    [Serializable]
    public class NSwagClient
    {
        #region Properties

        public string ClassCode { get; set; }

        public string ClassName { get; set; }

        public string Namespace { get; set; }

        public string SourceItemSpec { get; set; }

        #endregion
    }
}