using System;
using System.IO;
using System.Reflection;
using NJsonSchema.Infrastructure;
using NSwag.API;
using NSwag.API.Attributes;
using NSwag.MSBuild.Sources.Replacer;

namespace NSwag.MSBuild.Extensions
{
    public static class NSwagAttributesExtensions
    {
        #region Static members

        public static string GetDocument(this NSwagClientAttribute attribute, Replacer replacer)
        {
            var path = attribute.GetDocumentPath(replacer);
            switch (attribute.Source)
            {
                case DocumentSource.Local:
                    if (!File.Exists(path)) throw new FileNotFoundException("Swagger document not found", path);
                    return File.ReadAllText(path);
                case DocumentSource.Remote:
                    return DynamicApis.HttpGetAsync(path).Result;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static uint GetDocumentCacheKey(this NSwagClientAttribute attribute, Replacer replacer)
        {
            var path = attribute.GetDocumentPath(replacer);
            return (uint)attribute.Source.GetHashCode().MergeHash(path.GetHashCode());
        }

        public static string GetDocumentPath(this NSwagClientAttribute attribute, Replacer replacer)
        {
            return replacer.Replace(attribute.DocumentPath);
        }

        public static Type GetReferenceType(this TypeSourceAttribute attribute, Replacer replacer)
        {
            var typeName = replacer.Replace(attribute.TypeName);
            if (string.IsNullOrEmpty(attribute.AssemblyPath)) return Type.GetType(typeName);
            var assemblySource = replacer.Replace(attribute.AssemblyPath);
            var assembly = Assembly.LoadFrom(assemblySource);

            return assembly.GetType(typeName);
        }

        #endregion
    }
}