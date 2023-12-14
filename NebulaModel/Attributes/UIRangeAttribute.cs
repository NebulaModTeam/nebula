#region

using System;

#endregion

namespace NebulaModel.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class UIRangeAttribute : Attribute
{
    public readonly float Max;
    public readonly float Min;
    public readonly bool Slider;

    public UIRangeAttribute(float min, float max, bool slider = false)
    {
        Min = min;
        Max = max;
        Slider = slider;
    }
}
