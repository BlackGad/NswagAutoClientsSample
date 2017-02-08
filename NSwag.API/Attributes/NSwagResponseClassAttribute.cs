using System;

namespace NSwag.API.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class NSwagResponseClassAttribute : Attribute
    {
        #region Properties

        public bool Generate { get; set; }
        public bool Inject { get; set; }
        public string Name { get; set; }

        #endregion
    }
}