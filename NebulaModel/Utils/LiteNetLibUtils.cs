using LiteNetLib.Utils;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using System;
using System.Linq;
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

        public static void RegisterAllPacketProcessorsInCallingAssembly(NetPacketProcessor packetProcessor)
        {
            var processors = Assembly.GetCallingAssembly().GetTypes()
                .Where(t => t.GetCustomAttributes(typeof(RegisterPacketProcessorAttribute), true).Length > 0);

            foreach (Type type in processors)
            {
                var packetProcessorInterface = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPacketProcessor<>));
                if (packetProcessorInterface != null)
                {
                    Type packetType = packetProcessorInterface.GetGenericArguments().FirstOrDefault();
                    Console.WriteLine($"Registering {type.Name} to process packet of type: {packetType.Name}");

                    // Create instance of the processor
                    Type delegateType = typeof(Action<,>).MakeGenericType(packetType, typeof(NebulaConnection));
                    object processor = Activator.CreateInstance(type);
                    Delegate callback = Delegate.CreateDelegate(delegateType, processor, type.GetMethod("ProcessPacket"));

                    // Register our processor callback to the PacketProcessor
                    Type subscribeGenericType = typeof(Action<,>).MakeGenericType(packetType, typeof(NebulaConnection));

                    // TODO: Find a better way to get the "SubscribeReusable" that as the Action<T, TUserData> param.
                    MethodInfo method = packetProcessor.GetType().GetMethods().Where(m => m.Name == "SubscribeReusable").ToArray()[1];

                    MethodInfo generic = method.MakeGenericMethod(packetType, typeof(NebulaConnection));
                    generic.Invoke(packetProcessor, new object[] { callback });
                }
            }
        }
    }
}
