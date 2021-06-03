using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Processors;
using NebulaWorld;
using NebulaWorld.Planet;

namespace NebulaClient.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    class VegeMinedProcessor : IPacketProcessor<VegeMinedPacket>
    {
        public void ProcessPacket(VegeMinedPacket packet, NebulaConnection conn)
        {
            if (packet.FactoryIndex >= 0 && GameMain.data.factories[packet.FactoryIndex] != null && GameMain.data.factories[packet.FactoryIndex].vegePool != null)
            {
                using (PlanetManager.EventFromServer.On())
                {
                    SimulatedWorld.OnVegetationMined(packet);
                }
            }
        }
    }
}
