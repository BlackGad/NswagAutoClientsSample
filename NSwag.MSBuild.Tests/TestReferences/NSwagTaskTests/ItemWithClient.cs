using NSwag.API;
using NSwag.API.Attributes;

namespace Application.Clients
{
    [NSwagClient(DeclarationType.PartialClass, DocumentSource.Local, @"{build.TargetDir}\TestReferences\NSwagTaskTests\swagger.json", GenerateDTO = true)]
    //[NSwagClient(DeclarationType.PartialClass, DocumentSource.Local, @"{nuget.package.id}\swagger.json", GenerateDTO = true)]
    //[NSwagOperationNameGenerator(@"NSwag.CodeGeneration.OperationNameGenerators.SingleClientFromOperationIdOperationNameGenerator", AssemblyPath = @"{build.TargetDir}\NSwag.CodeGeneration.dll")]
    //[NSwagOperationNameGenerator(@"NSwag.CodeGeneration.OperationNameGenerators.SingleClientFromOperationIdOperationNameGenerator, NSwag.CodeGeneration")]
    //[NSwagOperationNameGenerator(@"NSwag.MSBuild.Tests.Tests.CustomOperationNameGenerator", AssemblyPath = @"{build.TargetDir}\NSwag.MSBuild.Tests.dll")]
    [NSwagAdditionalNamespace("Test.Namespace.Alpha")]
    [NSwagAdditionalNamespace("Test.Namespace.Bravo")]
    public partial class TestClient
    {
    }
}
