using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld;

namespace NebulaClient.PacketProcessors.Factory
{
    [RegisterPacketProcessor]
    public class EntityPlacedProcessor: IPacketProcessor<EntityPlaced>
    {
        public void ProcessPacket(EntityPlaced packet, NebulaConnection conn)
        {
            SimulatedWorld.PlaceEntity(packet);
        }
    }
}
