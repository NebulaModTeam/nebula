#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;

#endregion

namespace NebulaNetwork.PacketProcessors.Players;

[RegisterPacketProcessor]
internal class RemoveDroneOrdersProcessor : PacketProcessor<RemoveDroneOrdersPacket>
{
    protected override void ProcessPacket(RemoveDroneOrdersPacket packet, NebulaConnection conn)
    {
        if (packet.QueuedEntityIds == null)
        {
            return;
        }
        foreach (var t in packet.QueuedEntityIds)
        {
            GameMain.mainPlayer.mecha.droneLogic.serving.Remove(t);
        }
    }
}
