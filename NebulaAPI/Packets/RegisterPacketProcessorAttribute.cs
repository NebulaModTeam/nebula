#region

using System;

#endregion

namespace NebulaAPI;

/// <summary>
///     Registers packet processors. Make sure to register your assembly using NebulaModAPI.RegisterPackets
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RegisterPacketProcessorAttribute : Attribute
{
}
