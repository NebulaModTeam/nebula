using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Attributes;
using NebulaModel.Packets.Factory.Inserter;

namespace NebulaClient.PacketProcessors.Factory.Inserter
{
    [RegisterPacketProcessor]
    class InserterFilterUpdateProcessor : IPacketProcessor<InserterFilterUpdatePacket>
    {
        public void ProcessPacket(InserterFilterUpdatePacket packet, NebulaConnection conn)
        {
            InserterComponent[] pool = GameMain.localPlanet?.factory?.factorySystem?.inserterPool;
            if (pool != null && packet.InserterIndex != -1 && packet.InserterIndex < pool.Length && pool[packet.InserterIndex].id != -1)
            {
                pool[packet.InserterIndex].filter = packet.ItemId;
                int entityId = pool[packet.InserterIndex].entityId;
                GameMain.localPlanet.factory.entitySignPool[entityId].iconId0 = (uint)packet.ItemId;
                GameMain.localPlanet.factory.entitySignPool[entityId].iconType = ((packet.ItemId <= 0) ? 0U : 1U);
            }
        }
    }
}