#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Belt;
using NebulaWorld;
using NebulaWorld.Factory;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Belt;

[RegisterPacketProcessor]
internal class BeltUpdatePutItemOnProcessor : PacketProcessor<BeltUpdatePutItemOnPacket>
{
    protected override void ProcessPacket(BeltUpdatePutItemOnPacket packet, NebulaConnection conn)
    {
        if (!BeltManager.TryPutItemOnBelt(packet))
        {
            Multiplayer.Session.Belts.RegiserbeltPutdownPacket(packet);
        }
    }
}
