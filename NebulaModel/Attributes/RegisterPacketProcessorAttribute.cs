using System;

namespace NebulaModel.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RegisterPacketProcessorAttribute : Attribute
    {
        public RegisterPacketProcessorAttribute(Type PacketType)
        {

        }
    }
}
