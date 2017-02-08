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
    }
}