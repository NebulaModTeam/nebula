using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Belt;
using NebulaModel.Packets;
using NebulaWorld.Factory;

namespace NebulaClient.PacketProcessors.Factory.Belt
{
    [RegisterPacketProcessor]
    class BeltUpdatePutItemOnProcessor : PacketProcessor<BeltUpdatePutItemOnPacket>
    {
        public override void ProcessPacket(BeltUpdatePutItemOnPacket packet, NebulaConnection conn)
        {
            if (GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.cargoTraffic != null)
            {
                using (FactoryManager.EventFromServer.On())
                {
                    GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.cargoTraffic.PutItemOnBelt(packet.BeltId, packet.ItemId);
                }
            }
        }
    }
}