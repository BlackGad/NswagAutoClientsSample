using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NSwag.MSBuild.Sources.Replacer
{
    public class Replacer : List<Replacement>
    {
        #region Members

        public string Replace(string input)
        {
            const string groupName = nameof(groupName);
            var pattern = @"(?<" + groupName + ">{[^}]+})";
            var regex = new Regex(pattern);

            if (input == null) throw new ArgumentNullException("input");

            var evaluator = new MatchEvaluator(match =>
            {
                return this.FirstOrDefault(r => r.IsValid(match.Value))?.GetReplacement(match.Value) ??
                       match.Value;
            });

            return regex.Replace(input, evaluator);
        }

        #endregion
    }
}