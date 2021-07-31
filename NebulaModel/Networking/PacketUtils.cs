using Mirror;
using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking.Serialization;
using NebulaModel.Packets;
using NebulaModel.Utils;
using System;
using System.Linq;
using System.Reflection;

namespace NebulaModel.Networking
{
    public static class PacketUtils
    {
        public static void RegisterAllPacketNestedTypes(NetPacketProcessor packetProcessor)
        {
            var nestedTypes = AssembliesUtils.GetTypesWithAttribute<RegisterNestedTypeAttribute>();
            foreach (Type type in nestedTypes)
            {
                Console.WriteLine($"Registering Nested Type: {type.Name}");
                if (type.IsClass)
                {
                    // TODO: Find a better way to get the "NetPacketProcessor.RegisterNestedType" that as the Func<T> param instead of by index.
                    MethodInfo registerMethod = packetProcessor.GetType()
                        .GetMethods()
                        .Where(m => m.Name == nameof(NetPacketProcessor.RegisterNestedType))
                        .ToArray()[2]
                        .MakeGenericMethod(type);

                    MethodInfo delegateMethod = packetProcessor.GetType().GetMethod(nameof(NetPacketProcessor.CreateNestedClassInstance)).MakeGenericMethod(type);
                    var funcType = typeof(Func<>).MakeGenericType(type);
                    var callback = Delegate.CreateDelegate(funcType, packetProcessor, delegateMethod);
                    registerMethod.Invoke(packetProcessor, new object[] { callback });
                }
                else if (type.IsValueType)
                {
                    MethodInfo method = typeof(NetPacketProcessor).GetMethod(nameof(NetPacketProcessor.RegisterNestedType), Type.EmptyTypes);
                    MethodInfo generic = method.MakeGenericMethod(type);
                    generic.Invoke(packetProcessor, null);
                }
                else
                {
                    Log.Error($"Could not register nested type: {type.Name}. Must be a class or struct.");
                }
            }
        }

        public static void RegisterAllPacketProcessorsInCallingAssembly(NetPacketProcessor packetProcessor, bool isMasterClient)
        {
            var processors = Assembly.GetCallingAssembly().GetTypes()
                .Where(t => t.GetCustomAttributes(typeof(RegisterPacketProcessorAttribute), true).Length > 0);

            MethodInfo method = packetProcessor.GetType().GetMethods()
                .Where(m => m.Name == nameof(NetPacketProcessor.SubscribeReusable))
                .Where(m => m.IsGenericMethod && m.GetGenericArguments().Length == 2)
                .FirstOrDefault();

            foreach (Type type in processors)
            {
                if (type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(PacketProcessor<>))
                {
                    Type packetType = type.BaseType.GetGenericArguments().FirstOrDefault();
                    Console.WriteLine($"Registering {type.Name} to process packet of type: {packetType.Name}");

                    // Create instance of the processor
                    Type delegateType = typeof(Action<,>).MakeGenericType(packetType, typeof(NetworkConnection));
                    object processor = Activator.CreateInstance(type);
                    Delegate callback = Delegate.CreateDelegate(delegateType, processor, type.GetMethod(nameof(PacketProcessor<object>.ProcessPacket)));

                    // Initialize processor
                    type.BaseType.GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(processor, new object[] { isMasterClient });

                    // Register our processor callback to the PacketProcessor
                    Type subscribeGenericType = typeof(Action<,>).MakeGenericType(packetType, typeof(NetworkConnection));
                    MethodInfo generic = method.MakeGenericMethod(packetType, typeof(NetworkConnection));
                    generic.Invoke(packetProcessor, new object[] { callback });
                }
                else
                {
                    Log.Warn($"{type.FullName} registered, but doesn't implement {typeof(PacketProcessor<>).FullName}");
                }
            }
        }
    }
}
