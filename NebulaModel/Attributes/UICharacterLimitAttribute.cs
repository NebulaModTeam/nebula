using System;

namespace NebulaModel.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class UICharacterLimitAttribute : Attribute
    {
        public readonly int Max;

        public UICharacterLimitAttribute(int max)
        {
            Max = max;
        }
    }
}
