using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Inserter;

namespace NebulaNetwork.PacketProcessors.Factory.Inserter
{
    [RegisterPacketProcessor]
    class InserterFilterUpdateProcessor : PacketProcessor<InserterFilterUpdatePacket>
    {
        public override void ProcessPacket(InserterFilterUpdatePacket packet, NebulaConnection conn)
        {
            InserterComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factorySystem?.inserterPool;
            if (pool != null && packet.InserterIndex != -1 && packet.InserterIndex < pool.Length && pool[packet.InserterIndex].id != -1)
            {
                pool[packet.InserterIndex].filter = packet.ItemId;
                int entityId = pool[packet.InserterIndex].entityId;
                GameMain.galaxy.PlanetById(packet.PlanetId).factory.entitySignPool[entityId].iconId0 = (uint)packet.ItemId;
                GameMain.galaxy.PlanetById(packet.PlanetId).factory.entitySignPool[entityId].iconType = ((packet.ItemId <= 0) ? 0U : 1U);
            }
        }
    }
}