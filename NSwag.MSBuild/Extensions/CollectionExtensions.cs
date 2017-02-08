using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NSwag.MSBuild.Extensions
{
    public static class CollectionExtensions
    {
        #region Static members

        public static IEnumerable<T> Enumerate<T>(this IEnumerable<T> enumeration)
        {
            return enumeration ?? Enumerable.Empty<T>();
        }

        public static IEnumerable<T> Enumerate<T>(this object instance)
        {
            var enumeration = instance as IEnumerable;
            if (enumeration == null) return Enumerable.Empty<T>();

            return enumeration.OfType<T>();
        }

        #endregion
    }
}