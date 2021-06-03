using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Processors;
using NebulaWorld;
using NebulaWorld.Planet;

namespace NebulaHost.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    class VegeMinedProcessor : IPacketProcessor<VegeMinedPacket>
    {
        public void ProcessPacket(VegeMinedPacket packet, NebulaConnection conn)
        {
            if (packet.FactoryIndex >= 0 && GameMain.data.factories[packet.FactoryIndex] != null)
            {
                using (PlanetManager.EventFromClient.On())
                {
                    //GameMain.data.factories[packet.FactorytIndex]?.RemoveVegeWithComponents(packet.VegeId);
                    SimulatedWorld.OnVegetationMined(packet);
                }
            }
        }
    }
}
