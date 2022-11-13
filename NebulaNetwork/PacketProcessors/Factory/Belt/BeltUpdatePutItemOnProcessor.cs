using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Belt;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Factory.Belt
{
    [RegisterPacketProcessor]
    internal class BeltUpdatePutItemOnProcessor : PacketProcessor<BeltUpdatePutItemOnPacket>
    {
        public override void ProcessPacket(BeltUpdatePutItemOnPacket packet, NebulaConnection conn)
        {
            if (!Multiplayer.Session.Belts.TryPutItemOnBelt(packet))
            {
                Multiplayer.Session.Belts.RegiserbeltPutdownPacket(packet);
            }
        }
    }
}