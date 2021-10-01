using System;

namespace NebulaAPI
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public class HidePacketInDebugLogsAttribute : Attribute { }
}
