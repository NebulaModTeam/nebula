using System;

namespace NebulaModel.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class UIControlAttribute : Attribute
    {
        public readonly string displayName;

        public UIControlAttribute(string displayName)
        {
            this.displayName = displayName;
        }
    }
}
