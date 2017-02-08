using System;
using System.Diagnostics;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Moq;
using NSwag.MSBuild.Tests.TestReferences.NSwagTaskTests;

namespace NSwag.MSBuild.Tests.Common
{
    public static class BuildEngineRunner
    {
        #region Static members

        public static T Create<T>() where T : Task, new()
        {
            var engineMock = new Mock<IBuildEngine>();
            engineMock.Setup(engine => engine.LogMessageEvent(It.IsAny<BuildMessageEventArgs>()))
                      .Callback<BuildMessageEventArgs>(ev =>
                      {
                          Debug.WriteLine(ev.Message);
                          Console.WriteLine(ev.Message);
                      });

            engineMock.Setup(engine => engine.ProjectFileOfTaskNode)
                      .Returns(() => DataSource.ProjectFilePath);

            engineMock.Setup(engine => engine.LogErrorEvent(It.IsAny<BuildErrorEventArgs>()))
                      .Callback<BuildErrorEventArgs>(ev =>
                      {
                          Debug.WriteLine("E: " + ev.Message);
                          Console.WriteLine("E: " + ev.Message);
                      });
            var task = new T
            {
                BuildEngine = engineMock.Object
            };
            return task;
        }

        #endregion
    }
}