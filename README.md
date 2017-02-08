**Summary** 

Project uses roslyn and msbuild features to generate client code onfly from compilable .cs files.

**Project structure** 
- NSwag.API shared project with NSwagAttributes definition
- NSwag.MSBuild contains several tasks definition to accomplish code generation
- NSwag.MSBuild.Tests few unit tests for MSBuild Tasks
- Application.Clients test victim
- .CommonData contains definition for nuget package

**How to use** 
1. Build MSBuildTask.sln solution
2. Build Nuget package from .CommonData\Packages\
3. Create new c# project
4. Install Nuget package from step 2
5. Add NSwagClientAttribute to any class in project from step 3
6. Compile
7. Optional step is to look into build warnings. Task will notify you that some files contains NSwag client definitions and have no ```<Generator>MSBuild:UpdateSwaggerClients</Generator>``` msbuild item metadata. This metadata adds ability to change client definition code on fly. Don't forget to reload project after manual change.

**Available attributes**

*NSwagClientAttribute* - main client declaration attribute

Constructor parameters:
- *DeclarationType* - how code generator will interpret current class. (Behavior will be changed in future)
	- Generator - current class definition is not participate in clients name and base class name selection
	- PartialClass - the name of current class definition will be treated as clients name
	- BaseClientClass - the name of current class definition will be treated as clients base class name
- *DocumentSource* - the way how *DocumentPath* will be resolved
	- Local - resolve swagger document from local file 
	- Remote - resolve swagger document from web url
- *DocumentPath* - path to document source. Supports variable replacements.
	- {build.*} - will be resolved with current MSBuild properties (supported properties: Configuration, PlatformTarget, ProjectDir/Ext/Name, SolutionDir/Ext/Name, TargetDir/Ext/Name. To extend with your own variables you can specify CustomLocations MSBuild property with key value pairs separated with semicolon)
	- {env.*} - Current PC Environment variables
	- {nuget.*} - where * is Nuget package ID within current solution. Returns package source folder(last found version of package)

Optional parameters:
- *GenerateDTO* - generate or not DTO

*NSwagAdditionalNamespaceAttribute* - multiple definitions allowed

Constructor parameters:
	- *Namespace* - additional namespace to include into generated client code

*NSwagExceptionClassAttribute*

Optional parameters:
	- Generate - generate or not exception class definitions in generated code
	- Name - exception class name
	
*NSwagResponseClassAttribute*

Optional parameters:
	- Generate - generate or not response class definitions in generated code
	- Name - response class name
	- Inject - wrap client responses with ResponseClass or not (will be removed)
	
*NSwagTypeNameGeneratorAttribute* - generator will try to load this type and execute it for client code generation

Constructor parameters:
	- TypeName - partial or full type name that implement NJsonSchema.ITypeNameGenerator interface
	
Optional parameters:
	- AssemblyPath - path to assembly where type is located
	
*NSwagOperationNameGeneratorAttribute* - generator will try to load this type and execute it for client code generation

Constructor parameters:
	- TypeName - partial or full type name that implement NJsonSchema.IOperationNameGenerator interface
	
Optional parameters:
	- AssemblyPath - path to assembly where type is located
	
**Future** 
Unknown for now. This method require VSIX extension to fully handle generated code display in VS and proper <Generator>MSBuild:UpdateSwaggerClients</Generator> metadata set.