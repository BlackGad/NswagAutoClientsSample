using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NSwag.MSBuild.Extensions
{
    public static class XDocumentExtensions
    {
        #region Constants

        private const string DynamicIncludeTargetNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        #endregion

        #region Static members

        public static void AddCompileItem(this XDocument doc, string sourceItemSpec, string relativeGeneratedFilePathToProject)
        {
            doc.AddItem("Compile", sourceItemSpec, relativeGeneratedFilePathToProject);
        }

        public static void AddNoneItem(this XDocument doc, string sourceItemSpec, string relativeGeneratedFilePathToProject)
        {
            doc.AddItem("None", sourceItemSpec, relativeGeneratedFilePathToProject);
        }

        public static XDocument CreateDynamicIncludeTarget()
        {
            var target = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
                                       new XElement(XName.Get("Project", DynamicIncludeTargetNamespace),
                                                    new XAttribute("ToolsVersion", "4.0"),
                                                    new XElement(XName.Get("ItemGroup", DynamicIncludeTargetNamespace))
                                           ));

            return target;
        }

        private static void AddItem(this XDocument doc, string itemType, string sourceItemSpec, string relativeGeneratedFilePathToProject)
        {
            doc.Root
               .Descendants(XName.Get("ItemGroup", DynamicIncludeTargetNamespace))
               .First()
               .Add(new XElement(XName.Get(itemType, DynamicIncludeTargetNamespace),
                                 new XAttribute("Include", relativeGeneratedFilePathToProject),
                                 new XElement(XName.Get("DependentUpon", DynamicIncludeTargetNamespace), sourceItemSpec)));
        }

        #endregion
    }
}