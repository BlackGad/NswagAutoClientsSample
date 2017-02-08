using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using NSwag.API;
using NSwag.API.Attributes;
using NSwag.MSBuild.Sources;

namespace NSwag.MSBuild.Extensions
{
    public static class RoslynExtensions
    {
        #region Static members

        public static Tuple<AttributeArgumentSyntax, object> EvaluateExpression(this AttributeArgumentSyntax syntax)
        {
            object value;
            return syntax.Expression.TryEvaluateArgument(out value)
                ? new Tuple<AttributeArgumentSyntax, object>(syntax, value)
                : null;
        }

        public static Attribute GetAttribute(this AttributeSyntax attribute, Type attributeType)
        {
            var ctorDirectArguments = attribute.ArgumentList
                                               .Arguments
                                               .TakeWhile(a => a.NameEquals == null && a.NameColon == null)
                                               .Select(a => a.EvaluateExpression())
                                               .ToList();

            var ctorColonArguments = attribute.ArgumentList
                                              .Arguments
                                              .SkipWhile(a => a.NameEquals == null && a.NameColon == null)
                                              .TakeWhile(a => a.NameEquals == null && a.NameColon != null)
                                              .Select(a => a.EvaluateExpression())
                                              .ToList();

            if (ctorDirectArguments.Any(a => a == null)) return null;
            if (ctorColonArguments.Any(a => a == null)) return null;

            Attribute result = null;
            foreach (var c in attributeType.GetConstructors())
            {
                var ctorParams = c.GetParameters();
                var valueSequence = new Lazy<object>[ctorParams.Length];

                if (ctorDirectArguments.Count + ctorColonArguments.Count > ctorParams.Length) return null;
                for (var i = 0; i < ctorDirectArguments.Count; i++)
                {
                    var value = ctorDirectArguments[i].Item2;
                    if (!ctorParams[i].ParameterType.IsInstanceOfType(value)) return null;
                    valueSequence[i] = new Lazy<object>(() => value);
                }
                var remainingCtorParams = ctorParams.Skip(ctorDirectArguments.Count).ToArray();
                for (var i = 0; i < ctorColonArguments.Count; i++)
                {
                    var name = ctorColonArguments[i].Item1.NameColon.Name.Identifier.ValueText;
                    var value = ctorColonArguments[i].Item2;
                    var mappedProperty = remainingCtorParams.FirstOrDefault(p => Equals(p.Name, name) && p.ParameterType.IsInstanceOfType(value));
                    if (mappedProperty == null) return null;
                    valueSequence[ctorDirectArguments.Count + i] = new Lazy<object>(() => value);
                }

                for (int i = 0; i < valueSequence.Length; i++)
                {
                    var param = ctorParams[i];
                    var value = valueSequence[i];
                    if (value == null && param.HasDefaultValue) valueSequence[i] = new Lazy<object>(() => param.DefaultValue);
                }

                if (valueSequence.Any(v => v == null)) return null;
                try
                {
                    result = c.Invoke(valueSequence.Select(v => v.Value).ToArray()) as Attribute;
                }
                catch (Exception)
                {
                    //Nothing
                }
                //On this stage most possible constructor already found so break anyway
                break;
            }

            if (result == null) return null;

            var namedArguments = attribute.ArgumentList
                                          .Arguments
                                          .SkipWhile(a => a.NameEquals == null)
                                          .Select(a => a.EvaluateExpression())
                                          .ToList();
            if (namedArguments.Any(a => a == null)) return null;

            var properties = attributeType.GetProperties();
            foreach (var namedArgument in namedArguments)
            {
                var name = namedArgument.Item1.NameEquals.Name.Identifier.ValueText;
                var value = namedArgument.Item2;
                var property = properties.FirstOrDefault(p => p.CanWrite &&
                                                              Equals(p.Name, name) &&
                                                              p.PropertyType.IsInstanceOfType(namedArgument.Item2));
                if (property == null) return null;

                property.SetValue(result, value);
            }

            return result;
        }

        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this ClassDeclarationSyntax classDeclarationSyntax)
            where TAttribute : Attribute
        {
            return classDeclarationSyntax.GetAttributes(typeof(TAttribute)).OfType<TAttribute>();
        }

        public static IEnumerable<Attribute> GetAttributes(this ClassDeclarationSyntax classDeclarationSyntax, Type attributeType)
        {
            foreach (var attribute in classDeclarationSyntax.GetAttributesSyntax(attributeType))
            {
                yield return attribute.GetAttribute(attributeType);
            }
        }

        public static IEnumerable<AttributeSyntax> GetAttributesSyntax<T>(this ClassDeclarationSyntax classDeclarationSyntax) where T : Attribute
        {
            return classDeclarationSyntax.GetAttributesSyntax(typeof(T));
        }

        public static IEnumerable<AttributeSyntax> GetAttributesSyntax(this ClassDeclarationSyntax classDeclarationSyntax, Type attributeType)
        {
            var allAttributes = classDeclarationSyntax.AttributeLists.SelectMany(a => a.Attributes).ToList();
            return allAttributes.Where(a => Equals(a.Name.ToString().CutAttributeSuffix(),
                                                   attributeType.Name.CutAttributeSuffix()));
        }

        public static ParsedAttribute<T>[] GetMultipleAttributes<T>(this ClassDeclarationSyntax classDeclarationSyntax) where T : Attribute
        {
            var syntax = classDeclarationSyntax.GetAttributesSyntax<T>().ToArray();
            return syntax.Select(s => new ParsedAttribute<T>(s)).ToArray();
        }

        public static ParsedAttribute[] GetMultipleAttributes(this ClassDeclarationSyntax classDeclarationSyntax, Type attributeType)
        {
            var syntax = classDeclarationSyntax.GetAttributesSyntax(attributeType).ToArray();
            var resultType = typeof(ParsedAttribute<>).MakeGenericType(attributeType);

            var array = Array.CreateInstance(resultType, syntax.Length);
            for (int index = 0; index < syntax.Length; index++)
            {
                var arrayValue = Activator.CreateInstance(resultType, syntax[index]);
                array.SetValue(arrayValue, index);
            }

            return (ParsedAttribute[])array;
        }

        public static string GetNamespace(this ClassDeclarationSyntax classDeclarationSyntax)
        {
            var namespaceDeclaration = classDeclarationSyntax.Parent as NamespaceDeclarationSyntax;
            if (namespaceDeclaration == null) return null;
            return namespaceDeclaration.Name.ToString();
        }

        public static ParsedAttribute<T> GetSingleAttribute<T>(this ClassDeclarationSyntax classDeclarationSyntax) where T : Attribute
        {
            var syntax = classDeclarationSyntax.GetAttributesSyntax<T>().ToArray();
            if (syntax.Length != 1) return null;
            return new ParsedAttribute<T>(syntax.Single());
        }

        public static ParsedAttribute GetSingleAttribute(this ClassDeclarationSyntax classDeclarationSyntax, Type attributeType)
        {
            var syntax = classDeclarationSyntax.GetAttributesSyntax(attributeType).ToArray();
            if (syntax.Length != 1) return null;
            var resultType = typeof(ParsedAttribute<>).MakeGenericType(attributeType);
            return (ParsedAttribute)Activator.CreateInstance(resultType, syntax.Single());
        }

        public static ParsedAttribute<T> ToParsed<T>(this AttributeSyntax syntax) where T : Attribute
        {
            return new ParsedAttribute<T>(syntax);
        }

        private static string CutAttributeSuffix(this string attributeName)
        {
            if (attributeName.EndsWith(nameof(Attribute))) return attributeName.Substring(0, attributeName.Length - nameof(Attribute).Length);
            return attributeName;
        }

        private static bool TryEvaluateArgument(this ExpressionSyntax expressionSyntax, out object value)
        {
            value = null;
            try
            {
                var scriptOptions = ScriptOptions.Default
                                                 .WithReferences(Assembly.GetExecutingAssembly())
                                                 .WithReferences(typeof(NSwagClientAttribute).Assembly)
                                                 .WithImports(typeof(DeclarationType).Namespace);
                var evaluatedValue = CSharpScript.EvaluateAsync(expressionSyntax.ToString(), scriptOptions).Result;
                value = evaluatedValue;
                return true;
            }
            catch
            {
                //Could not evaluate attribute argument value (for example unknown for this scoped code constants usage. In future it could be improved)
                return false;
            }
        }

        #endregion
    }
}