using System;

namespace NebulaModel.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class HidePacketInDebugLogsAttribute : Attribute { }
}
