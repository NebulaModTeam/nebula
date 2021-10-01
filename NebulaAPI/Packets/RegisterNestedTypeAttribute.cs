using System;

namespace NebulaAPI
{
    /// <summary>
    /// Registers custom data structure serializer. Make sure to register your assembly using NebulaModAPI.RegisterPackets
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class RegisterNestedTypeAttribute : Attribute { }
}
