using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using NebulaAPI.Interfaces;


namespace NebulaModel.Networking.Serialization;

public class NebulaNetSerializer : NetSerializer
{
    public override void Register<T>()
    {
        RegisterInternal<T>();
    }

    protected override ClassInfo<T> RegisterInternal<T>()
    {
        if (ClassInfo<T>.Instance != null)
            return ClassInfo<T>.Instance;

        // if type has dictionary
        // Generate the reader and writer for the dictionary
        // call RegisterNestedType on dictionary type
        // then call base.RegisterInternal()

        Type type = typeof(T);
        var props = type.GetProperties(
            BindingFlags.Instance |
            BindingFlags.Public |
            // BindingFlags.NonPublic |
            BindingFlags.GetProperty |
            BindingFlags.SetProperty);

        foreach (var prop in props)
        {
            // Not using the interface as that's implementing by other types that don't necessarily provide the same functionality, or may be thread unsafe.
            if (!prop.PropertyType.IsGenericType)
            {
                continue;
            }

            if (prop.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var genericArguments = prop.PropertyType.GenericTypeArguments;

                // call RegisterDictionary<RegisterDictionary<TDict, TKey, TValue>>()
                // on the dictionary to generate a serializer for it.
                typeof(NebulaNetSerializer)
                    .GetMethod(nameof(NebulaNetSerializer.RegisterDictionary), BindingFlags.NonPublic | BindingFlags.Instance)!
                    .MakeGenericMethod([prop.PropertyType, genericArguments[0], genericArguments[1]])
                    .Invoke(this, null);
            }
        }

        return base.RegisterInternal<T>();
    }

    ///<remarks>This can potentially be moved to its own class,
    /// and be registered through an attribute, when/if we'd like to add more custom containers.</remarks>
    protected void RegisterDictionary<TDict, TKey, TValue>() where TDict : Dictionary<TKey, TValue> where TKey : new() where TValue : new()
    {
        var readKeyDelegate = GetTypeGetter<TKey>();
        var writeKeyDelegate = GetTypePutter<TKey>();

        var readValueDelegate = GetTypeGetter<TValue>();
        var writeValueDelegate = GetTypePutter<TValue>();

        // Logic for writing a dictionary
        Action<NetDataWriter, TDict> writerDelegate = (writer, dict) =>
        {
            writer.Put(dict.Count);
            foreach (var kvp in dict)
            {
                writeKeyDelegate(writer, kvp.Key);
                writeValueDelegate(writer, kvp.Value);
            }
        };

        // Logic for reading a dictionary
        Func<NetDataReader, TDict> readerDelegate = (reader) =>
        {
            var dict = new Dictionary<TKey, TValue>();

            int count = reader.GetInt();

            for (int i = 0; i < count; i++)
            {
                // Could arguably make this also use the library's fastcall but that's too much boilerplate for now.
                var key = readKeyDelegate(reader);
                var value = readValueDelegate(reader);

                dict.Add(key, value);
            }

            return (TDict)dict;
        };

        RegisterNestedType(writerDelegate, readerDelegate);
    }

    protected Func<NetDataReader, TValue> GetTypeGetter<TValue>() where TValue : new()
    {
        var valueType = typeof(TValue);

        // INetSerializable implementers are treated as valueTypes in this case, since they get their own direct Get or Put call.
        if (valueType.GetInterface(nameof(INetSerializable)) != null)
            return GetNetSerializableTypeGetter<TValue>();

        if (valueType.IsValueType)
            return GetValueTypeGetter<TValue>();

        if (valueType.IsArray && valueType.GetElementType()!.IsValueType)
            return GetValueTypeGetter<TValue>();

        // Otherwise if it's a custom type, get a reader for that.
        return GetCustomTypeGetter<TValue>();
    }

    private Func<NetDataReader, TValue> GetNetSerializableTypeGetter<TValue>() where TValue : new()
    {
        var valueType = typeof(TValue);
        var readerType = typeof(NetDataReader);

        var getterMethods = readerType.GetMethods().Where(info => info.Name == "Get");
        MethodInfo getterMethod;
        if (valueType.IsClass)
            getterMethod = getterMethods.FirstOrDefault(info => info.GetParameters().Length == 1)?.MakeGenericMethod(valueType);
        else
            getterMethod = getterMethods.FirstOrDefault(info => info.GetParameters().Length == 0)?.MakeGenericMethod(valueType);

        if (getterMethod == null)
            throw new NullReferenceException($"Could not find a valid 'get' serializer for type {valueType.Name}");

        var instanceParam = Expression.Parameter(readerType, "reader");
        var constructorParam = Expression.Parameter(typeof(Func<TValue>), "constructor");

        // Class type call, you have to pass a constructor delegate to this one
        MethodCallExpression getCall;
        if (valueType.IsClass)
        {
            var constructorDelegate = () => new TValue();
            getCall = Expression.Call(instanceParam, getterMethod, constructorParam);
            var lambda = Expression.Lambda<Func<NetDataReader, Func<TValue>, TValue>>(
                getCall, [instanceParam, constructorParam]).Compile();

            // Just calling the constructor version in an internal lambda so the calling code doesn't have to worry about it.
            return (reader) => lambda(reader, constructorDelegate);
        }

        // non-class call
        getCall = Expression.Call(instanceParam, getterMethod!);
        return Expression.Lambda<Func<NetDataReader, TValue>>(getCall, [instanceParam]).Compile();
    }

    protected Func<NetDataReader, TValue> GetCustomTypeGetter<TValue>() where TValue : new()
    {
        // If it's not a value type, get the serializer's fastcall instance for that special type
        var fastCallReader = RegisterInternal<TValue>().Read;

        // // Our read function
        return (reader) =>
        {
            TValue value = new();
            fastCallReader(value, reader);
            return value;
        };
    }

    protected Func<NetDataReader, TValue> GetValueTypeGetter<TValue>()
    {
        var valueType = typeof(TValue);
        var readerType = typeof(NetDataReader);
        var getterMethods = readerType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(info => info.Name.StartsWith("Get"));

        var getterMethod = getterMethods?.FirstOrDefault(info => info.ReturnType == valueType);

        if (getterMethod == null)
            throw new NullReferenceException($"Could not find a valid 'get' serializer for type {valueType.Name}");

        // You can view what ExpressionTrees generate by breaking onto them.
        var instanceParam = Expression.Parameter(readerType, "reader");
        var call = Expression.Call(instanceParam, getterMethod!);
        return Expression.Lambda<Func<NetDataReader, TValue>>(call, instanceParam).Compile();
    }

    protected Action<NetDataWriter, TValue> GetTypePutter<TValue>()
    {
        var valueType = typeof(TValue);

        // INetSerializable implementers are treated as valueTypes in this case, since they get their own direct Put call.
        if (valueType.GetInterface(nameof(INetSerializable)) != null)
            return GetValueTypePutter<TValue>();

        if (valueType.IsValueType)
            return GetValueTypePutter<TValue>();

        if (valueType.IsArray && valueType.GetElementType()!.IsValueType)
            return GetValueTypePutter<TValue>();

        // Otherwise if it's a custom type, get a reader for that.
        return GetCustomTypePutter<TValue>();
    }

    private Action<NetDataWriter, TValue> GetCustomTypePutter<TValue>()
    {
        var fastCallWriter = RegisterInternal<TValue>().Write;

        // // Our read function
        return (writer, value) =>
        {
            fastCallWriter(value, writer);
        };
    }

    protected Action<NetDataWriter, TValue> GetValueTypePutter<TValue>()
    {
        var valueType = typeof(TValue);
        var writerType = typeof(NetDataWriter);
        var putterMethods = writerType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(info => info.Name.StartsWith("Put")).ToArray();

        var putterMethod = (MethodInfo)Type.DefaultBinder!.SelectMethod(BindingFlags.Default, putterMethods, [valueType], null);

        // If we don't find a direct put, try the generic one
        if (putterMethod == null)
            putterMethod = putterMethods.FirstOrDefault(info => info.IsGenericMethod)?.MakeGenericMethod([valueType]);

        if (putterMethod == null)
            throw new NullReferenceException($"Could not find a valid 'put' serializer for type {valueType.Name}");

        // You can view what ExpressionTrees generate by breaking onto them.
        var instanceParam = Expression.Parameter(writerType, "reader");
        var valueParam = Expression.Parameter(valueType, "value");
        var call = Expression.Call(instanceParam, putterMethod, [valueParam]);
        return Expression.Lambda<Action<NetDataWriter, TValue>>(call, instanceParam, valueParam).Compile();
    }
}
