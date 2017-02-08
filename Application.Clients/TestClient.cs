using NSwag.API;
using NSwag.API.Attributes;

namespace Application.Clients
{
    [NSwagClient(DeclarationType.PartialClass, DocumentSource.Local, @"d:\Projects\NswagAutoClientsSample\NSwag.MSBuild.Tests\TestReferences\NSwagTaskTests\swagger.json", GenerateDTO = true)]
    [NSwagAdditionalNamespace("Test1.Namespace.Alpha")]
    [NSwagAdditionalNamespace("Test.Namespace.Bravo")]
    public partial class TestClient
    {
        public TestClient()
        { 
              
        }
    }
}
