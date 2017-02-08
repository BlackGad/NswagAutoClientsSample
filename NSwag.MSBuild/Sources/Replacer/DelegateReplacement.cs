using System;
using NSwag.API;

namespace NSwag.MSBuild.Sources.Replacer
{
    public class DelegateReplacement : Replacement
    {
        private readonly Func<string, string> _replaceFunc;
        private readonly Func<string, bool> _validatePredicate;

        #region Constructors

        public DelegateReplacement(string key, Func<string, string> replaceFunc, Func<string, bool> validatePredicate = null) : base(key)
        {
            if (replaceFunc == null) throw new ArgumentNullException(nameof(replaceFunc));
            _validatePredicate = validatePredicate;
            _replaceFunc = replaceFunc;
        }

        #endregion

        #region Override members

        protected override bool Validate(string value)
        {
            return _validatePredicate?.Invoke(value) ?? true;
        }

        protected override string Replace(string value)
        {
            return _replaceFunc(value);
        }

        #endregion
    }
}