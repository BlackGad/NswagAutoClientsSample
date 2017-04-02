using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NJsonSchema;
using NSwag.API;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.OperationNameGenerators;
using NSwag.MSBuild.Extensions;
using NSwag.MSBuild.Sources;
using NSwag.MSBuild.Sources.Attributes;
using NSwag.MSBuild.Sources.Replacer;

namespace NSwag.MSBuild.Tasks
{
    public class NSwagGenerateCodeTask : Task
    {
        private readonly List<ITaskItem> _cachedItems;

        #region Constructors

        public NSwagGenerateCodeTask()
        {
            _cachedItems = new List<ITaskItem>();
        }

        #endregion

        #region Properties

        [Output]
        public ITaskItem[] CachedItems
        {
            get { return _cachedItems.ToArray(); }
        }

        [TaskReplacerProperty]
        public string Configuration { get; set; }

        public string CustomLocations { get; set; }

        [Required]
        public string IntermediateOutputPath { get; set; }

        [Required]
        public ITaskItem[] NSwagClients { get; set; }

        [TaskReplacerProperty]
        public string PlatformTarget { get; set; }

        [TaskReplacerProperty]
        public string ProjectDir { get; set; }

        [TaskReplacerProperty]
        public string ProjectExt { get; set; }

        [TaskReplacerProperty]
        public string ProjectName { get; set; }

        [TaskReplacerProperty]
        public string SolutionDir { get; set; }

        [TaskReplacerProperty]
        public string SolutionExt { get; set; }

        [TaskReplacerProperty]
        public string SolutionName { get; set; }

        [TaskReplacerProperty]
        public string TargetDir { get; set; }

        [TaskReplacerProperty]
        public string TargetExt { get; set; }

        [TaskReplacerProperty]
        public string TargetName { get; set; }

        #endregion

        #region Override members

        public override bool Execute()
        {
            _cachedItems.Clear();
            var replacer = new Replacer();

            replacer.Add(new EnvironmentReplacement());
            replacer.Add(CreateNugetReplacer());
            replacer.Add(CreateBuildReplacer());

            foreach (var client in NSwagClients)
            {
                var restoredClient = client.Restore();
                try
                {
                    var parsedClient = restoredClient.Parse();
                    Log.LogMessage(MessageImportance.Normal, $"Analyzing Swagger document for '{parsedClient.ClassName}' client");

                    var declaration = parsedClient.Declaration.Parsed;
                    var documentCacheFilePath = Path.Combine(IntermediateOutputPath,
                                                             Path.GetDirectoryName(parsedClient.SourceItemSpec),
                                                             string.Join(".",
                                                                         Path.GetFileNameWithoutExtension(parsedClient.SourceItemSpec),
                                                                         parsedClient.ClassName,
                                                                         "json"));
                    Path.GetDirectoryName(documentCacheFilePath).EnsureDirectoryExist();

                    string swaggerDocument;
                    if (File.Exists(documentCacheFilePath))
                    {
                        Log.LogMessage(MessageImportance.Normal, "  - Document available in cache");
                        swaggerDocument = File.ReadAllText(documentCacheFilePath);
                        Log.LogMessage(MessageImportance.Normal, "  - Document loaded from cache");
                    }
                    else
                    {
                        Log.LogMessage(MessageImportance.Normal,
                                       $"  - Fetching document from '{declaration.GetDocumentPath(replacer)}' in {declaration.Source} mode");
                        try
                        {
                            swaggerDocument = declaration.GetDocument(replacer);

                            Log.LogMessage(MessageImportance.Normal, "  - Document acquired. Caching...");
                            File.WriteAllText(documentCacheFilePath, swaggerDocument);
                            _cachedItems.Add(new TaskItem(documentCacheFilePath));
                            Log.LogMessage(MessageImportance.Normal, "  - Document cached");
                        }
                        catch (Exception e)
                        {
                            Log.LogErrorFromException(new Exception("Cannot fetch swagger document", e));
                            throw;
                        }
                    }

                    try
                    {
                        var code = GenerateClientCode(parsedClient, swaggerDocument, replacer);
                        var filePath = client.GetMetadata("FullPath");
                        Log.LogMessage(MessageImportance.Normal, $"  - Saving generated code to {filePath}");
                        File.WriteAllText(filePath, code);
                    }
                    catch (Exception e)
                    {
                        Log.LogError($"Cannot generate code. Details: {e.Message}");
                        throw;
                    }
                }
                catch (Exception)
                {
                    //Nothing
                }
            }

            return true;
        }

        #endregion

        #region Members

        private DictionaryReplacement CreateBuildReplacer()
        {
            var buildReplacer = new DictionaryReplacement("build");
            var replacerProperties = GetType().GetProperties()
                                              .Where(p => p.GetCustomAttribute<TaskReplacerPropertyAttribute>() != null);

            Log.LogMessage(MessageImportance.Low, "Common replace properties:");
            foreach (var replacerProperty in replacerProperties)
            {
                var value = replacerProperty.GetValue(this) as string;
                if (value != null)
                {
                    buildReplacer.Set(replacerProperty.Name, value);
                    Log.LogMessage(MessageImportance.Low, $"  * {replacerProperty.Name} = {value}");
                }
            }

            if (!string.IsNullOrWhiteSpace(CustomLocations))
            {
                Log.LogMessage(MessageImportance.Low, "  - Custom replace properties:");
                foreach (var customProperty in CustomLocations.Split(';'))
                {
                    var customPropertyParts = customProperty.Split('=');
                    if (customPropertyParts.Length == 2)
                    {
                        buildReplacer.Set(customPropertyParts[0], customPropertyParts[1]);
                        Log.LogMessage(MessageImportance.Low, $"    * {customPropertyParts[0]} = {customPropertyParts[1]}");
                    }
                }
            }
            return buildReplacer;
        }

        private DelegateReplacement CreateNugetReplacer()
        {
            return new DelegateReplacement("nuget",
                                           value =>
                                           {
                                               var targetFolders = new List<string>();
                                               if (!string.IsNullOrEmpty(ProjectDir))
                                               {
                                                   targetFolders.Add(ProjectDir);
                                                   targetFolders.Add(ProjectDir + "\\..\\");
                                               }
                                               if (!string.IsNullOrEmpty(SolutionDir))
                                               {
                                                   targetFolders.Add(SolutionDir);
                                               }

                                               targetFolders.Add(AppDomain.CurrentDomain.BaseDirectory);
                                               targetFolders.Add(AppDomain.CurrentDomain.BaseDirectory + "\\..\\");

                                               var packageFolders = NugetExtensions.FindPackageLattestVersionFolders(value,
                                                                                                                     targetFolders.ToArray());
                                               return packageFolders.FirstOrDefault()?.Path;
                                           });
        }

        string GenerateClientCode(NSwagClientParsed declaration, string documentJson, Replacer replacer)
        {
            Log.LogMessage(MessageImportance.Normal, "  - Generating client code");
            var document = SwaggerDocument.FromJsonAsync(documentJson).Result;

            Log.LogMessage(MessageImportance.Normal, "  - Generator settings:");

            var settings = new SwaggerToCSharpClientGeneratorSettings
            {
                GenerateDtoTypes = declaration.Declaration.Parsed.GenerateDTO,
                CSharpGeneratorSettings =
                {
                    Namespace = declaration.Namespace
                },
                AdditionalNamespaceUsages = declaration.AdditionalNamespaces.Enumerate().Select(a => a.Parsed.Namespace).ToArray()
            };

            if (declaration.Declaration.Parsed.Type == DeclarationType.BaseClientClass)
            {
                settings.ClientBaseClass = declaration.ClassName;
                Log.LogMessage(MessageImportance.Normal, $"    * Base class name: {settings.ClientBaseClass}");
            }

            if (declaration.Declaration.Parsed.Type == DeclarationType.PartialClass)
            {
                settings.ClassName = declaration.ClassName;
                Log.LogMessage(MessageImportance.Normal, $"    * Partial class name: {settings.ClassName}");
            }

            if (declaration.OperationNameGenerator != null)
            {
                var valueType = declaration.OperationNameGenerator.Parsed?.GetReferenceType(replacer);
                if (valueType == null)
                {
                    var exception = new InvalidOperationException("Could not resolve OperationNameGenerator type");
                    Log.LogErrorFromException(exception);
                    throw exception;
                }

                settings.OperationNameGenerator = (IOperationNameGenerator)Activator.CreateInstance(valueType);
            }

            settings.OperationNameGenerator = settings.OperationNameGenerator ?? new SingleClientFromOperationIdOperationNameGenerator();

            Log.LogMessage(MessageImportance.Normal, $"    * Namespace name: {settings.CSharpGeneratorSettings.Namespace}");
            Log.LogMessage(MessageImportance.Normal, $"    * Generate DTO: {settings.GenerateDtoTypes}");
            Log.LogMessage(MessageImportance.Normal, $"    * Additional namespaces: {string.Join(";", settings.AdditionalNamespaceUsages)}");
            Log.LogMessage(MessageImportance.Normal, $"    * Operator name generator: {settings.OperationNameGenerator.GetType().Name}");

            if (declaration.TypeNameGenerator != null)
            {
                var valueType = declaration.TypeNameGenerator.Parsed?.GetReferenceType(replacer);
                if (valueType == null)
                {
                    var exception = new InvalidOperationException("Could not resolve TypeNameGenerator type");
                    Log.LogErrorFromException(exception);
                    throw exception;
                }
                settings.CodeGeneratorSettings.TypeNameGenerator = (ITypeNameGenerator)Activator.CreateInstance(valueType);
                Log.LogMessage(MessageImportance.Normal,
                               $"    * Type name generator: {settings.CodeGeneratorSettings.TypeNameGenerator.GetType().Name}");
            }

            if (declaration.ExceptionClass != null)
            {
                settings.ExceptionClass = declaration.ExceptionClass.Parsed.Name;
                settings.GenerateExceptionClasses = declaration.ExceptionClass.Parsed.Generate;
                Log.LogMessage(MessageImportance.Normal, $"    * Exception class name: {settings.ExceptionClass}");
                if (settings.GenerateExceptionClasses)
                    Log.LogMessage(MessageImportance.Normal, $"    * Generate exception classes: {settings.GenerateExceptionClasses}");
            }

            if (declaration.ResponseClass?.Parsed.Inject == true)
            {
                if (!string.IsNullOrEmpty(declaration.ResponseClass.Parsed.Name)) settings.ResponseClass = declaration.ResponseClass.Parsed.Name;
                settings.GenerateResponseClasses = declaration.ResponseClass.Parsed.Generate;
                settings.WrapResponses = declaration.ResponseClass.Parsed.Inject;
                Log.LogMessage(MessageImportance.Normal, $"    * Response class name: {settings.ResponseClass}");
                if (settings.GenerateExceptionClasses)
                    Log.LogMessage(MessageImportance.Normal, $"    * Generate response classes: {settings.GenerateResponseClasses}");
            }

            var generator = new SwaggerToCSharpClientGenerator(document, settings);
            var result = generator.GenerateFile();
            Log.LogMessage(MessageImportance.Normal, "  - Code generated");
            return result;
        }

        #endregion
    }
}