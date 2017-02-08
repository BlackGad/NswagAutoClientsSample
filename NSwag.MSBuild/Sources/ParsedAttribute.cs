using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSwag.MSBuild.Extensions;

namespace NSwag.MSBuild.Sources
{
    public abstract class ParsedAttribute
    {
        #region Constructors

        protected ParsedAttribute(AttributeSyntax attributeSyntex, Type realType)
        {
            if (attributeSyntex == null) throw new ArgumentNullException(nameof(attributeSyntex));
            AttributeSyntex = attributeSyntex;
            RealType = realType;
        }

        #endregion

        #region Properties

        public AttributeSyntax AttributeSyntex { get; }

        public Type RealType { get; }

        public string Source
        {
            get { return AttributeSyntex.ToFullString(); }
        }

        #endregion
    }

    public class ParsedAttribute<TBaseType> : ParsedAttribute where TBaseType : Attribute
    {
        private TBaseType _parsed;

        #region Constructors

        public ParsedAttribute(AttributeSyntax attributeSyntex) : this(attributeSyntex, null)
        {
        }

        public ParsedAttribute(AttributeSyntax attributeSyntex, Type realType) : base(attributeSyntex, realType)
        {
            if (realType != null && !typeof(TBaseType).IsAssignableFrom(realType))
                throw new ArgumentException($"{realType.Name} type must be inherited from {typeof(TBaseType).Name}");
        }

        #endregion

        #region Properties

        public TBaseType Parsed
        {
            get { return _parsed ?? (_parsed = (TBaseType)AttributeSyntex.GetAttribute(RealType ?? typeof(TBaseType))); }
        }

        #endregion
    }
}