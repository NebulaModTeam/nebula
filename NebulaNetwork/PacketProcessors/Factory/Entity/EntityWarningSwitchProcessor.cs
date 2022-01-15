using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;

namespace NebulaNetwork.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    class EntityWarningSwitchProcessor : PacketProcessor<EntityWarningSwitchPacket>
    {
        public override void ProcessPacket(EntityWarningSwitchPacket packet, NebulaConnection conn)
        {
            PlanetFactory factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
            EntityData[] pool = factory?.entityPool;
            if (pool != null && packet.EntityId > 0 && packet.EntityId < pool.Length && pool[packet.EntityId].id == packet.EntityId)
            {
                if (IsHost)
                {
                    if (packet.Enable && pool[packet.EntityId].warningId == 0)
                    {
                        GameMain.data.warningSystem.NewWarningData(factory.index, packet.EntityId, 0);
                    }
                    else if (!packet.Enable)
                    {
                        GameMain.data.warningSystem.RemoveWarningData(pool[packet.EntityId].warningId);
                        pool[packet.EntityId].warningId = 0;
                    }
                }
                else
                {
                    //Use dummy warningId on client side
                    pool[packet.EntityId].warningId = packet.Enable ? 1 : 0;
                }
            }
        }
    }
}
