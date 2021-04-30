using System.Reflection;
using System.Linq;
using System;

namespace NebulaModel.Attributes
{
    public static class AttributeExtension
    {
        public static T GetCustomAttribute<T>(this PropertyInfo property) where T : Attribute
        {
            return property.GetCustomAttributes(true)
                .Select(att => att as T)
                .Where(att => att != null)
                .FirstOrDefault();
        }
    }
}
