#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion

namespace NebulaModel.Utils;

public static class AssembliesUtils
{
    public static IEnumerable<Type> GetTypesWithAttributeInAssembly<T>(Assembly assembly) where T : Attribute
    {
        return assembly.GetTypes().Where(t => t.GetCustomAttributes(typeof(T), true).Length > 0);
    }

    public static IEnumerable<Assembly> GetNebulaAssemblies()
    {
        return AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("Nebula"));
    }

    public static Assembly GetAssemblyByName(string name)
    {
        return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.StartsWith($"{name}."));
    }
}
