using System;

namespace NSwag.MSBuild.Sources.Replacer
{
    public class EnvironmentReplacement : DelegateReplacement
    {
        #region Constructors

        public EnvironmentReplacement() : base("env", value => Environment.GetEnvironmentVariable(value ?? string.Empty))
        {
        }

        #endregion
    }
}