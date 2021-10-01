using System;

namespace NebulaAPI
{
    /// <summary>
    /// Registers packet processors. Make sure to register your assembly using NebulaModAPI.RegisterPackets
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RegisterPacketProcessorAttribute : Attribute { }
}
