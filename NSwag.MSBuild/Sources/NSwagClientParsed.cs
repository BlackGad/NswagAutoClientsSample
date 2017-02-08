using NSwag.API.Attributes;

namespace NSwag.MSBuild.Sources
{
    public class NSwagClientParsed : NSwagClient
    {
        #region Properties

        public ParsedAttribute<NSwagAdditionalNamespaceAttribute>[] AdditionalNamespaces { get; set; }
        public ParsedAttribute<NSwagClientAttribute> Declaration { get; set; }
        public ParsedAttribute<NSwagExceptionClassAttribute> ExceptionClass { get; set; }
        public ParsedAttribute<NSwagOperationNameGeneratorAttribute> OperationNameGenerator { get; set; }
        public ParsedAttribute<NSwagResponseClassAttribute> ResponseClass { get; set; }
        public ParsedAttribute<NSwagTypeNameGeneratorAttribute> TypeNameGenerator { get; set; }

        #endregion
    }
}