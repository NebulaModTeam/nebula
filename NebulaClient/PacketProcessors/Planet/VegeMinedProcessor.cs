using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets;
using NebulaWorld;
using NebulaWorld.Planet;

namespace NebulaClient.PacketProcessors.Planet
{
    // Processes events for mining vegetation or veins
    [RegisterPacketProcessor]
    class VegeMinedProcessor : PacketProcessor<VegeMinedPacket>
    {
        public override void ProcessPacket(VegeMinedPacket packet, NebulaConnection conn)
        {
            if (GameMain.galaxy.PlanetById(packet.PlanetId)?.factory != null && GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.vegePool != null)
            {
                using (PlanetManager.EventFromServer.On())
                {
                    SimulatedWorld.OnVegetationMined(packet);
                }
            }
        }
    }
}
