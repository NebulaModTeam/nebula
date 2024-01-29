#region

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NebulaAPI;
using NebulaAPI.Networking;
using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking.Serialization;
using NebulaModel.Utils;

#endregion

namespace NebulaModel.Networking;

public static class PacketUtils
{
    public static void RegisterAllPacketNestedTypesInAssembly(Assembly assembly, NetPacketProcessor packetProcessor)
    {
        var nestedTypes = AssembliesUtils.GetTypesWithAttributeInAssembly<RegisterNestedTypeAttribute>(assembly);
        var isAPIAssemblies = NebulaModAPI.TargetAssemblies.Contains(assembly);
        foreach (var type in nestedTypes)
        {
            if (isAPIAssemblies)
            {
                Log.Info($"Registering Nested Type: {type.Name}");
            }
            else
            {
                Log.Debug($"Registering Nested Type: {type.Name}");
            }

            if (type.IsClass)
            {
                var registerMethod = packetProcessor.GetType().GetMethods()
                    .Where(m => m.Name == nameof(NetPacketProcessor.RegisterNestedType))
                    .FirstOrDefault(m =>
                        m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType.Name.Equals(typeof(Func<>).Name))!
                    .MakeGenericMethod(type);

                var constructorMethod = typeof(Activator)
                    .GetMethods()
                    .Where((info => info.GetParameters().Length == 0))
                    .FirstOrDefault()!
                    .MakeGenericMethod(type);

                // create a Func<T> delegate from the object's constructor to pass into NetPacketProcessor.RegisterNestedType
                var funcType = typeof(Func<>).MakeGenericType(type);
                var constructorDelegate = Delegate.CreateDelegate(funcType, constructorMethod);

                // Invoke NetPacketProcessor.RegisterNestedType<T>(Func<T> constructor)
                registerMethod.Invoke(packetProcessor, new object[] { constructorDelegate });
            }
            else if (type.IsValueType)
            {
                var method = typeof(NetPacketProcessor).GetMethod(nameof(NetPacketProcessor.RegisterNestedType),
                    Type.EmptyTypes);
                var generic = method.MakeGenericMethod(type);
                generic.Invoke(packetProcessor, null);
            }
            else
            {
                Log.Error($"Could not register nested type: {type.Name}. Must be a class or struct.");
            }
        }
    }

    private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
    {
        while (toCheck != null && toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == cur)
            {
                return true;
            }

            toCheck = toCheck.BaseType;
        }

        return false;
    }

    public static void RegisterAllPacketProcessorsInAssembly(Assembly assembly, NetPacketProcessor packetProcessor,
        bool isMasterClient)
    {
        var processors = assembly.GetTypes()
            .Where(t => t.GetCustomAttributes(typeof(RegisterPacketProcessorAttribute), true).Length > 0);

        var method = packetProcessor.GetType()
            .GetMethods()
            .Where(m => m.Name == nameof(NetPacketProcessor.SubscribeReusable))
            .FirstOrDefault(m => m.IsGenericMethod && m.GetGenericArguments().Length == 2);

        var isAPIAssemblies = NebulaModAPI.TargetAssemblies.Contains(assembly);
        foreach (var type in processors)
        {
            if (IsSubclassOfRawGeneric(typeof(BasePacketProcessor<>), type))
            {
                var packetType = type.BaseType.GetGenericArguments().FirstOrDefault();
                if (isAPIAssemblies)
                {
                    Log.Info($"Registering {type.Name} to process packet of type: {packetType.Name}");
                }
                else
                {
                    Log.Debug($"Registering {type.Name} to process packet of type: {packetType.Name}");
                }

                // Create instance of the processor
                var delegateType = typeof(Action<,>).MakeGenericType(packetType, typeof(INebulaConnection));
                var processor = Activator.CreateInstance(type);
                var callback = Delegate.CreateDelegate(delegateType, processor,
                    type.GetMethod(nameof(BasePacketProcessor<object>.ProcessPacket),
                        new[] { packetType, typeof(INebulaConnection) }));

                // Initialize processor
                type.BaseType.GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(processor, new object[] { isMasterClient });

                // Register our processor callback to the PacketProcessor
                var subscribeGenericType = typeof(Action<,>).MakeGenericType(packetType, typeof(INebulaConnection));
                var generic = method.MakeGenericMethod(packetType, typeof(INebulaConnection));
                generic.Invoke(packetProcessor, new object[] { callback });
            }
            else
            {
                Log.Warn($"{type.FullName} registered, but doesn't implement {typeof(BasePacketProcessor<>).FullName}");
            }
        }
    }

    public static void RegisterAllPacketProcessorsInCallingAssembly(NetPacketProcessor packetProcessor, bool isMasterClient)
    {
        RegisterAllPacketProcessorsInAssembly(Assembly.GetCallingAssembly(), packetProcessor, isMasterClient);
    }
}
