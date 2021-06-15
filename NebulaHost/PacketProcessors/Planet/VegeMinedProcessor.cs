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
            if (GameMain.galaxy.PlanetById(packet.PlanetId)?.factory != null)
            {
                using (PlanetManager.EventFromClient.On())
                {
                    SimulatedWorld.OnVegetationMined(packet);
                }
            }
        }
    }
}
