using System.Reflection;
using System.Linq;
using System;

namespace NebulaModel.Attributes
{
    public static class AttributeExtension
    {
        public static T GetCustomAttribute<T>(this MemberInfo member) where T : Attribute
        {
            return member.GetCustomAttributes(true)
                .OfType<T>()
                .FirstOrDefault();
        }
    }
}
