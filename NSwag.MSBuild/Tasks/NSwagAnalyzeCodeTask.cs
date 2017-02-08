using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSwag.API.Attributes;
using NSwag.MSBuild.Extensions;
using NSwag.MSBuild.Sources;

namespace NSwag.MSBuild.Tasks
{
    public class NSwagAnalyzeCodeTask : Task
    {
        private readonly List<ITaskItem> _cachedItems;
        private readonly List<ITaskItem> _nswagClients;

        #region Constructors

        public NSwagAnalyzeCodeTask()
        {
            _nswagClients = new List<ITaskItem>();
            _cachedItems = new List<ITaskItem>();
        }

        #endregion

        #region Properties

        [Output]
        public ITaskItem[] CachedItems
        {
            get { return _cachedItems.ToArray(); }
        }

        [Required]
        public ITaskItem[] Compile { get; set; }

        [Required]
        public string DynamicIncludeTarget { get; set; }

        public bool HandleGenerateMetadata { get; set; }

        [Required]
        public string IntermediateOutputPath { get; set; }

        [Output]
        public ITaskItem[] NSwagClients
        {
            get { return _nswagClients.ToArray(); }
        }

        #endregion

        #region Override members

        public override bool Execute()
        {
            Log.LogMessage(MessageImportance.Normal, "Analyzing project files for NSwag clients...");
            _nswagClients.Clear();
            _cachedItems.Clear();

            var projectDir = Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode) ?? string.Empty;
            projectDir = projectDir.TrimEnd('\\') + "\\";

            var dynamicIncludeTarget = XDocumentExtensions.CreateDynamicIncludeTarget();

            using (var cache = new CacheManager(IntermediateOutputPath, Log))
            {
                _cachedItems.Add(new TaskItem(cache.GetCachePath()));

                var compileItemsToAddGenerator = new List<ITaskItem>();
                var compileItemsToRemoveGenerator = new List<ITaskItem>();

                foreach (var item in Compile)
                {
                    Log.LogMessage(MessageImportance.Normal, $"Analyzing '{item.ItemSpec}' compiled item");

                    var clients = cache.GetCached(item);

                    if (clients == null)
                    {
                        clients = GetClients(item);
                        if (clients.Any() && string.IsNullOrEmpty(item.GetMetadata("Generator"))) compileItemsToAddGenerator.Add(item);
                        cache.Cache(item, clients);
                        Log.LogMessage(MessageImportance.Normal, "  - Source code analyzed. Results cached.");
                    }
                    else
                    {
                        Log.LogMessage(MessageImportance.Normal, "  - Results restored from cache");
                    }

                    if (!clients.Any())
                    {
                        if (Equals(item.GetMetadata("Generator"), "MSBuild:UpdateSwaggerClients"))
                        {
                            compileItemsToRemoveGenerator.Add(item);
                        }

                        Log.LogMessage(MessageImportance.Normal, "  - There is no NSwag declarations. Skipping.");
                        continue;
                    }

                    foreach (var client in clients)
                    {
                        Log.LogMessage(MessageImportance.Normal, $"  - Detected: {client.Namespace}.{client.ClassName} NSwag declaration");

                        var sourceItemSpec = client.SourceItemSpec;
                        var relativeSourceItemSpec = sourceItemSpec.IsAbsolutePath()
                            ? IOExtensions.MakeRelativePath(projectDir, sourceItemSpec)
                            : sourceItemSpec;

                        var generatedFilePathRelative = Path.Combine(Path.GetDirectoryName(relativeSourceItemSpec),
                                                                     string.Join(".",
                                                                                 Path.GetFileNameWithoutExtension(relativeSourceItemSpec),
                                                                                 client.ClassName,
                                                                                 "cs"));

                        var generatedFilePath = Path.Combine(IntermediateOutputPath, generatedFilePathRelative);

                        Log.LogMessage(MessageImportance.Normal, $"  - Shadow source code will be stored in '{generatedFilePath}'");

                        Path.GetDirectoryName(generatedFilePath).EnsureDirectoryExist();

                        var taskItem = new TaskItem(generatedFilePath);

                        var relativeGeneratedFilePath = generatedFilePath.IsAbsolutePath()
                            ? IOExtensions.MakeRelativePath(projectDir, generatedFilePath)
                            : generatedFilePath;

                        dynamicIncludeTarget.AddCompileItem(sourceItemSpec, relativeGeneratedFilePath);

                        client.Save(taskItem);
                        taskItem.SetMetadata("Source", item.GetMetadata("FullPath"));
                        _nswagClients.Add(taskItem);
                    }
                }

                var dynamicIncludeTargetPath = Path.Combine(IntermediateOutputPath, DynamicIncludeTarget);
                Path.GetDirectoryName(dynamicIncludeTargetPath).EnsureDirectoryExist();
                
                //Disabled for now
                //dynamicIncludeTarget.Save(dynamicIncludeTargetPath);

                _cachedItems.Add(new TaskItem(dynamicIncludeTargetPath));

                if (compileItemsToAddGenerator.Any() || compileItemsToRemoveGenerator.Any())
                {
                    foreach (var item in compileItemsToAddGenerator)
                    {
                        Log.LogWarning($"  - Compile item {item.ItemSpec} contains NSwag client declaration " +
                                       "but has no <Generator>MSBuild:UpdateSwaggerClients</Generator> attribute declaration");
                    }

                    foreach (var item in compileItemsToRemoveGenerator)
                    {
                        Log.LogWarning($"  - Compile item {item.ItemSpec} does not contain NSwag client declaration " +
                                       "but has <Generator>MSBuild:UpdateSwaggerClients</Generator> attribute declaration");
                    }
                }
            }

            return true;
        }

        #endregion

        #region Members

        public NSwagClient[] GetClients(ITaskItem compileItem)
        {
            var filePath = compileItem.GetMetadata("FullPath");
            if (!File.Exists(filePath)) return Enumerable.Empty<NSwagClient>().ToArray();

            var sourceCode = File.ReadAllText(filePath);
            var tree = CSharpSyntaxTree.ParseText(sourceCode);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            //Find all class declarations in file with NSwagClientAttribute declared
            //Only high level class definition is allowed
            var nswagClientClasses = root.DescendantNodes()
                                         .OfType<ClassDeclarationSyntax>()
                                         .Where(c => c.Parent is CompilationUnitSyntax ||
                                                     c.Parent is NamespaceDeclarationSyntax)
                                         .Where(c => c.GetAttributesSyntax<NSwagClientAttribute>().Enumerate().Any());

            return nswagClientClasses.Select(clientClass => new NSwagClient
            {
                ClassName = clientClass.Identifier.ValueText,
                Namespace = clientClass.GetNamespace(),
                ClassCode = clientClass.ToFullString(),
                SourceItemSpec = compileItem.ItemSpec
            }).ToArray();
        }

        #endregion
    }
}