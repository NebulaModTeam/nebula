using LiteNetLib.Utils;
using NebulaModel.Attributes;
using System;
using System.Reflection;

namespace NebulaModel.Utils
{
    public static class LiteNetLibUtils
    {
        public static void RegisterAllPacketNestedTypes(NetPacketProcessor packetProcessor)
        {
            var nestedTypes = AssembliesUtils.GetTypesWithAttribute<RegisterNestedTypeAttribute>();
            foreach (Type type in nestedTypes)
            {
                Console.WriteLine($"Registering Nested Type: {type.Name}");
                MethodInfo method = typeof(NetPacketProcessor).GetMethod(nameof(NetPacketProcessor.RegisterNestedType), Type.EmptyTypes);
                MethodInfo generic = method.MakeGenericMethod(type);
                generic.Invoke(packetProcessor, null);
            }
        }
    }
}
