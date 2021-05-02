using System;

namespace NebulaModel.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class UIRangeAttribute : Attribute
    {
        public readonly float Min;
        public readonly float Max;
        public readonly bool Slider;

        public UIRangeAttribute(float min, float max, bool slider = false)
        {
            Min = min;
            Max = max;
            Slider = slider;
        }
    }
}