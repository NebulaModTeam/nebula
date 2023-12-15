#region

using NebulaAPI.Packets;
using NebulaModel.DataStructures;

#endregion

namespace NebulaModel.Packets.Universe;

[HidePacketInDebugLogs]
public class DysonLaunchDataPacket
{
    public DysonLaunchDataPacket() { }

    public DysonLaunchDataPacket(DysonLaunchData data)
    {
        Data = data;
    }

    public DysonLaunchData Data { get; set; }
}
