using System;

namespace NebulaAPI
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RegisterPacketProcessorAttribute : Attribute { }
}
