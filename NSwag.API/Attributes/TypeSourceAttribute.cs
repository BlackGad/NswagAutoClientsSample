using System;

namespace NSwag.API.Attributes
{
    public abstract class TypeSourceAttribute : Attribute
    {
        #region Constructors

        protected TypeSourceAttribute(string typeName)
        {
            if (typeName == null) throw new ArgumentNullException(nameof(typeName));
            TypeName = typeName;
        }

        #endregion

        #region Properties

        public string AssemblyPath { get; set; }

        public string TypeName { get; }

        #endregion
    }
}