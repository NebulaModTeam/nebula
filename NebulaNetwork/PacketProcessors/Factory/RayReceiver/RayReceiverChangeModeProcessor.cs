using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.RayReceiver;

namespace NebulaNetwork.PacketProcessors.Factory.RayReceiver
{
    [RegisterPacketProcessor]
    class RayReceiverChangeModeProcessor : PacketProcessor<RayReceiverChangeModePacket>
    {
        public override void ProcessPacket(RayReceiverChangeModePacket packet, NebulaConnection conn)
        {
            PowerGeneratorComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.powerSystem.genPool;
            if (pool != null && packet.GeneratorId != -1 && packet.GeneratorId < pool.Length && pool[packet.GeneratorId].id != -1)
            {
                if (packet.Mode == RayReceiverMode.Electricity)
                {
                    pool[packet.GeneratorId].productId = 0;
                    pool[packet.GeneratorId].productCount = 0f;
                }
                else if (packet.Mode == RayReceiverMode.Photon)
                {
                    ItemProto itemProto = LDB.items.Select((int)GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.entityPool[pool[packet.GeneratorId].entityId].protoId);
                    pool[packet.GeneratorId].productId = itemProto.prefabDesc.powerProductId;
                }
            }
        }
    }
}
