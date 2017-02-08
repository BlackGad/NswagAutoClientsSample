using System.Collections.Generic;

namespace NSwag.MSBuild.Sources.Replacer
{
    public class DictionaryReplacement : Replacement
    {
        private readonly Dictionary<string, string> _dictionary;

        #region Constructors

        public DictionaryReplacement(string key) : base(key)
        {
            _dictionary = new Dictionary<string, string>();
        }

        #endregion

        #region Override members

        protected override bool Validate(string value)
        {
            return _dictionary.ContainsKey(value);
        }

        protected override string Replace(string value)
        {
            return _dictionary[value];
        }

        #endregion

        #region Members

        public void Set(string subKey, string value)
        {
            subKey = subKey.ToLowerInvariant();
            if (!_dictionary.ContainsKey(subKey)) _dictionary.Add(subKey, string.Empty);
            _dictionary[subKey] = value;
        }

        #endregion
    }
}