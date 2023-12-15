#region

using System;

#endregion

namespace NebulaModel.Networking;

public struct DelayedPacket
{
    public PendingPacket Packet { get; set; }
    public DateTime DueTime { get; set; }

    public DelayedPacket(PendingPacket packet, DateTime dueTime)
    {
        Packet = packet;
        DueTime = dueTime;
    }
}
