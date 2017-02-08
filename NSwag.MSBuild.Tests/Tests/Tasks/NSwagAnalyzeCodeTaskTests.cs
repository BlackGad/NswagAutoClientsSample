using NSwag.MSBuild.Extensions;
using NSwag.MSBuild.Tasks;
using NSwag.MSBuild.Tests.Common;
using NSwag.MSBuild.Tests.TestReferences.NSwagTaskTests;
using NUnit.Framework;

namespace NSwag.MSBuild.Tests.Tests.Tasks
{
    [TestFixture]
    class NSwagAnalyzeCodeTaskTests
    {
        [Test]
        public void Test()
        {
            var task = BuildEngineRunner.Create<NSwagAnalyzeCodeTask>();
            task.IntermediateOutputPath = DataSource.IntermediateOutputPath();
            task.DynamicIncludeTarget = DataSource.DynamicIncludeTarget;
            task.Compile = new[]
            {
                DataSource.ItemWithClientWithOutGenerator,
                //DataSource.ItemWithClientWithGenerator,
                //DataSource.ItemWithOutClientWithOutGenerator,
                DataSource.ItemWithOutClientWithGenerator
            };
            task.Execute();
            Assert.AreEqual(1, task.NSwagClients.Length);
            var client = task.NSwagClients[0];

            Assert.AreEqual(DataSource.ItemWithClientWithOutGenerator.ItemSpec, client.GetMetadata("Source"));

            var restoredClient = client.Restore();

            Assert.AreEqual("TestClient", restoredClient.ClassName);
            Assert.AreEqual("Application.Clients", restoredClient.Namespace);
            //Assert.NotNull(restoredClient.Declaration);
            //Assert.NotNull(restoredClient.AdditionalNamespaces);
            //Assert.AreEqual(2, restoredClient.AdditionalNamespaces.Length);
        }
    }
}