using System;

namespace NSwag.API.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NSwagOperationNameGeneratorAttribute : TypeSourceAttribute
    {
        #region Constructors

        public NSwagOperationNameGeneratorAttribute(string typeName)
            : base(typeName)
        {
        }

        #endregion
    }
}