using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NSwag.MSBuild.Tests.TestReferences.NSwagTaskTests
{
    public static class DataSource
    {
        #region Static members

        public static string DynamicIncludeTarget
        {
            get { return @"Dynamic.Include.target"; }
        }

        public static string IntermediateOutputPath(string key = null)
        {
            key = key ?? Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            return MakeAbsolutePath(Path.Combine("IntermediateOutput", key));
        }

        public static ITaskItem ItemWithClientWithGenerator
        {
            get
            {
                return new TaskItem(MakeAbsolutePath(@"TestReferences\NSwagTaskTests\ItemWithClient.cs"),
                                    new Dictionary<string, string>
                                    {
                                        { "Generator", "MSBuild:UpdateSwaggerClients" }
                                    });
            }
        }

        public static ITaskItem ItemWithClientWithOutGenerator
        {
            get { return new TaskItem(MakeAbsolutePath(@"TestReferences\NSwagTaskTests\ItemWithClient.cs")); }
        }

        public static ITaskItem ItemWithOutClientWithGenerator
        {
            get
            {
                return new TaskItem(MakeAbsolutePath(@"TestReferences\NSwagTaskTests\ItemWithOutClient.cs"),
                                    new Dictionary<string, string>
                                    {
                                        { "Generator", "MSBuild:UpdateSwaggerClients" }
                                    });
            }
        }

        public static ITaskItem ItemWithOutClientWithOutGenerator
        {
            get { return new TaskItem(MakeAbsolutePath(@"TestReferences\NSwagTaskTests\ItemWithOutClient.cs")); }
        }

        public static string ProjectFilePath
        {
            get { return MakeAbsolutePath(@"TestReferences\NSwagTaskTests\Test.csproj"); }
        }

        private static string MakeAbsolutePath(string relative)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relative);
        }

        #endregion
    }
}