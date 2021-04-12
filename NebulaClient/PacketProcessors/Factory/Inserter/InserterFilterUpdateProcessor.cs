using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory.Inserter;
using NebulaModel.Packets.Processors;

namespace NebulaClient.PacketProcessors.Factory.Inserter
{
    [RegisterPacketProcessor]
    class InserterFilterUpdateProcessor : IPacketProcessor<InserterFilterUpdatePacket>
    {
        public void ProcessPacket(InserterFilterUpdatePacket packet, NebulaConnection conn)
        {
            InserterComponent[] pool = GameMain.data.factories[packet.FactoryIndex]?.factorySystem?.inserterPool;
            if (pool != null && packet.InserterIndex != -1 && packet.InserterIndex < pool.Length && pool[packet.InserterIndex].id != -1)
            {
                pool[packet.InserterIndex].filter = packet.ItemId;
                int entityId = pool[packet.InserterIndex].entityId;
                GameMain.data.factories[packet.FactoryIndex].entitySignPool[entityId].iconId0 = (uint)packet.ItemId;
                GameMain.data.factories[packet.FactoryIndex].entitySignPool[entityId].iconType = ((packet.ItemId <= 0) ? 0U : 1U);
            }
        }
    }
}