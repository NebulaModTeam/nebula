#region

using System;

#endregion

namespace NebulaModel.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class UICharacterLimitAttribute : Attribute
{
    public readonly int Max;

    public UICharacterLimitAttribute(int max)
    {
        Max = max;
    }
}
