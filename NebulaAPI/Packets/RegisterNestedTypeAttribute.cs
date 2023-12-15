#region

using System;

#endregion

namespace NebulaAPI.Packets;

/// <summary>
///     Registers custom data structure serializer. Make sure to register your assembly using NebulaModAPI.RegisterPackets
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class RegisterNestedTypeAttribute : Attribute;
