using System;
using System.Linq;
using NSwag.MSBuild.Tasks;
using NSwag.MSBuild.Tests.Common;
using NSwag.MSBuild.Tests.TestReferences.NSwagTaskTests;
using NUnit.Framework;

namespace NSwag.MSBuild.Tests.Tests.Tasks
{
    [TestFixture]
    class NSwagGenerateCodeTaskTests
    {
        [Test]
        public void Test()
        {
            var analyzeTask = BuildEngineRunner.Create<NSwagAnalyzeCodeTask>();
            var intermediateOutputPath = DataSource.IntermediateOutputPath();

            analyzeTask.IntermediateOutputPath = intermediateOutputPath;
            analyzeTask.DynamicIncludeTarget = DataSource.DynamicIncludeTarget;
            analyzeTask.Compile = new[]
            {
                DataSource.ItemWithClientWithOutGenerator,
                DataSource.ItemWithOutClientWithOutGenerator
            };

            analyzeTask.Execute();

            var client = analyzeTask.NSwagClients.FirstOrDefault();
            Assert.NotNull(client);

            var generateTask = BuildEngineRunner.Create<NSwagGenerateCodeTask>();
            generateTask.IntermediateOutputPath = intermediateOutputPath;
            generateTask.TargetDir = AppDomain.CurrentDomain.BaseDirectory;
            generateTask.SolutionDir = @"d:\Projects\NswagAutoClientsSample\";
            generateTask.NSwagClients = new[] { client };
            generateTask.Execute();
        }
    }
}