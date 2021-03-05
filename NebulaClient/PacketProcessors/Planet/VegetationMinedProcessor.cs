using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Processors;
using NebulaWorld;

namespace NebulaClient.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    public class VegetationMinedProcessor : IPacketProcessor<VegeMined>
    {
        public void ProcessPacket(VegeMined packet, NebulaConnection conn)
        {
            SimulatedWorld.MineVegetable(packet);
        }
    }
}
