using System;

namespace NebulaModel.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class UIRangeAttribute : Attribute
    {
        public readonly float min;
        public readonly float max;

        public UIRangeAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
}