using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Inserter;

namespace NebulaNetwork.PacketProcessors.Factory.Inserter
{
    [RegisterPacketProcessor]
    internal class InserterItemUpdateProcessor : PacketProcessor<InserterItemUpdatePacket>
    {
        public override void ProcessPacket(InserterItemUpdatePacket packet, NebulaConnection conn)
        {
            InserterComponent[] pool = GameMain.galaxy.PlanetById(packet.PlanetId).factory?.factorySystem.inserterPool;
            if (pool != null && packet.InserterIndex != -1 && packet.InserterIndex < pool.Length && pool[packet.InserterIndex].id != -1)
            {
                pool[packet.InserterIndex].itemId = packet.ItemId;
                pool[packet.InserterIndex].itemCount = packet.ItemCount;
                pool[packet.InserterIndex].itemInc = packet.ItemInc;
                pool[packet.InserterIndex].stackCount = packet.StackCount;
            }
        }
    }
}