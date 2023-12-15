#region

using System;

#endregion

namespace NebulaAPI.Packets;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class HidePacketInDebugLogsAttribute : Attribute;
