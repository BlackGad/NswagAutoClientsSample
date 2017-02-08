using System;

namespace NSwag.API.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class NSwagAdditionalNamespaceAttribute : Attribute
    {
        #region Constructors

        public NSwagAdditionalNamespaceAttribute(string ns)
        {
            Namespace = ns;
        }

        #endregion

        #region Properties

        public string Namespace { get; }

        #endregion
    }
}