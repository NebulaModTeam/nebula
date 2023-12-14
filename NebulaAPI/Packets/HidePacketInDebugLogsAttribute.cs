#region

using System;

#endregion

namespace NebulaAPI;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class HidePacketInDebugLogsAttribute : Attribute
{
}
