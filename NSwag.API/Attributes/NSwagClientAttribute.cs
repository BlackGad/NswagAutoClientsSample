using System;

namespace NSwag.API.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NSwagClientAttribute : Attribute
    {
        #region Constructors

        public NSwagClientAttribute(DeclarationType type, DocumentSource documentSource, string documentPath)
        {
            Source = documentSource;
            if (documentPath == null) throw new ArgumentNullException(nameof(documentPath));
            DocumentPath = documentPath;
            Type = type;
        }

        #endregion

        #region Properties

        public string DocumentPath { get; }

        public bool GenerateDTO { get; set; }

        public DocumentSource Source { get; }

        public DeclarationType Type { get; }

        #endregion

        //    {
        //    switch (Source)
        //    var path = GetDocumentPath(replacer);
        //{

        //public string GetDocument(Replacer replacer)
        //        case DocumentSource.Local:

        //            if (!File.Exists(path)) throw new FileNotFoundException("Swagger document not found", path);
        //            return File.ReadAllText(path);
        //        case DocumentSource.Remote:
        //            throw new NotImplementedException();
        //        default:
        //            throw new ArgumentOutOfRangeException();
        //    }
        //}

        //public uint GetDocumentCacheKey(Replacer replacer)
        //{
        //    var path = replacer.Replace(DocumentPath);
        //    return (uint)Source.GetHashCode().MergeHash(path.GetHashCode());
        //}

        //public string GetDocumentPath(Replacer replacer)
        //{
        //    return replacer.Replace(DocumentPath);
        //}
    }
}