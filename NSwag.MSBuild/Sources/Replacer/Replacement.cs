using System;

namespace NSwag.MSBuild.Sources.Replacer
{
    public abstract class Replacement
    {
        private readonly string _key;

        #region Constructors

        protected Replacement(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            _key = key;
        }

        #endregion

        #region Members

        public string GetReplacement(string match)
        {
            var input = ParseInput(match);
            if (input == null) throw new InvalidOperationException();
            return Replace(input.Item2.ToLowerInvariant());
        }

        public bool IsValid(string match)
        {
            var input = ParseInput(match);
            if (input == null) return false;
            return string.Equals(input.Item1, _key, StringComparison.InvariantCultureIgnoreCase) &&
                   Validate(input.Item2.ToLowerInvariant());
        }

        protected abstract string Replace(string value);

        protected virtual bool Validate(string value)
        {
            return true;
        }

        private Tuple<string, string> ParseInput(string input)
        {
            input = input.TrimStart('{').TrimEnd('}');
            var firstDotPosition = input.IndexOf(".", StringComparison.InvariantCulture);
            if (firstDotPosition == -1) return null;
            return new Tuple<string, string>(input.Substring(0, firstDotPosition),
                                             input.Substring(firstDotPosition + 1));
        }

        #endregion
    }
}