using System;

namespace NSwag.API.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NSwagTypeNameGeneratorAttribute : TypeSourceAttribute
    {
        #region Constructors

        public NSwagTypeNameGeneratorAttribute(string typeName)
            : base(typeName)
        {
        }

        #endregion
    }
}