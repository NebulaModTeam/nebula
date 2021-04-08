using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory.RayReceiver;
using NebulaModel.Packets.Processors;

namespace NebulaClient.PacketProcessors.Factory.RayReceiver
{
    [RegisterPacketProcessor]
    class RayReceiverChangeModeProcessor : IPacketProcessor<RayReceiverChangeModePacket>
    {
        public void ProcessPacket(RayReceiverChangeModePacket packet, NebulaConnection conn)
        {
            PowerGeneratorComponent[] pool = GameMain.localPlanet?.factory?.powerSystem.genPool;
            if (pool != null && packet.GeneratorId != -1 && packet.GeneratorId < pool.Length && pool[packet.GeneratorId].id != -1)
            {
                if (packet.Mode == RayReceiverMode.Electricity)
                {
                    pool[packet.GeneratorId].productId = 0;
                    pool[packet.GeneratorId].productCount = 0f;
                }
                else if (packet.Mode == RayReceiverMode.Photon)
                {
                    ItemProto itemProto = LDB.items.Select((int)GameMain.localPlanet.factory.entityPool[pool[packet.GeneratorId].entityId].protoId);
                    pool[packet.GeneratorId].productId = itemProto.prefabDesc.powerProductId;
                }
            }
        }
    }
}
