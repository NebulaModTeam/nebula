using System;

namespace NebulaModel.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RegisterPacketProcessorAttribute : Attribute { }
}
