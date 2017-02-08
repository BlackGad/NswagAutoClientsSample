using System;
using System.Linq;
using AutoMapper;
using Microsoft.Build.Framework;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSwag.MSBuild.Sources;

namespace NSwag.MSBuild.Extensions
{
    public static class NSwagClientExtensions
    {
        #region Static members

        public static NSwagClientParsed Parse(this NSwagClient client)
        {
            var result = Mapper.Map<NSwagClientParsed>(client);
            var tree = CSharpSyntaxTree.ParseText(client.ClassCode);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var classDeclaration = root.DescendantNodes()
                                       .OfType<ClassDeclarationSyntax>()
                                       .FirstOrDefault();
            if (classDeclaration == null) throw new InvalidCastException("Class code declaration is corrupted");

            //A bit of reflection magic to automate attributes parsing.
            //Enumerate all properties, parse attribute type and automatically evaluate them from roslyn attribute syntax
            var allProperties = result.GetType()
                                      .GetProperties()
                                      .Where(p => typeof(ParsedAttribute).IsAssignableFrom(p.PropertyType) ||
                                                  typeof(ParsedAttribute[]).IsAssignableFrom(p.PropertyType));
            foreach (var property in allProperties)
            {
                object value = null;
                if (typeof(ParsedAttribute).IsAssignableFrom(property.PropertyType))
                {
                    var attributeType = property.PropertyType.GetGenericArguments()[0];
                    value = classDeclaration.GetSingleAttribute(attributeType);
                }

                if (typeof(ParsedAttribute[]).IsAssignableFrom(property.PropertyType))
                {
                    var attributeType = property.PropertyType.GetElementType().GetGenericArguments()[0];
                    value = classDeclaration.GetMultipleAttributes(attributeType);
                }
                property.SetValue(result, value);
            }

            return result;
        }

        public static NSwagClient Restore(this ITaskItem sourceItem)
        {
            var result = new NSwagClient();
            //A bit of reflection magic to automate properties assignment.
            result.GetType()
                  .GetProperties()
                  .Where(p => typeof(string).IsAssignableFrom(p.PropertyType))
                  .ToList()
                  .ForEach(p => p.SetValue(result, sourceItem.GetMetadata(nameof(NSwagClient) + p.Name)));

            return result;
        }

        public static void Save(this NSwagClient client, ITaskItem targetItem)
        {
            //A bit of reflection magic to automate properties assignment.
            client.GetType()
                  .GetProperties()
                  .Where(p => typeof(string).IsAssignableFrom(p.PropertyType))
                  .ToList()
                  .ForEach(p => targetItem.SetMetadata(nameof(NSwagClient) + p.Name, p.GetValue(client) as string));
        }

        #endregion

        #region Constructors

        static NSwagClientExtensions()
        {
            Mapper.Initialize(config => config.CreateMap<NSwagClient, NSwagClientParsed>());
        }

        #endregion
    }
}