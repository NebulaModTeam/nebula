using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NebulaModel.Utils
{
    public static class AssembliesUtils
    {
        public static IEnumerable<Type> GetTypesWithAttribute<T>() where T : Attribute
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith("Nebula"))
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetCustomAttributes(typeof(T), true).Length > 0);
        }

        public static Assembly GetAssemblyByName(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith($"{name}.")).FirstOrDefault();
        }
    }
}
