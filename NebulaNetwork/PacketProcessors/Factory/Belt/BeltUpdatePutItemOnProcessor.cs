using Mirror;
using NebulaModel.Attributes;
using NebulaModel.Packets;
using NebulaModel.Packets.Belt;
using NebulaWorld.Factory;

namespace NebulaNetwork.PacketProcessors.Factory.Belt
{
    [RegisterPacketProcessor]
    class BeltUpdatePutItemOnProcessor : PacketProcessor<BeltUpdatePutItemOnPacket>
    {
        public override void ProcessPacket(BeltUpdatePutItemOnPacket packet, NetworkConnection conn)
        {
            using (FactoryManager.IsIncomingRequest.On())
            {
                GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.cargoTraffic?.PutItemOnBelt(packet.BeltId, packet.ItemId);
            }
        }
    }
}