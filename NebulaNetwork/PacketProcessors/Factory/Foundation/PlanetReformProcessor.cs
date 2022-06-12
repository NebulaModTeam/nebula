using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Foundation;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Factory.Foundation
{
    [RegisterPacketProcessor]
    internal class PlanetReformProcessor : PacketProcessor<PlanetReformPacket>
    {
        public override void ProcessPacket(PlanetReformPacket packet, NebulaConnection conn)
        {
            using (Multiplayer.Session.Planets.IsIncomingRequest.On())
            {
                PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
                if (planet?.factory == null)
                {
                    return;
                }
                Multiplayer.Session.Factories.TargetPlanet = packet.PlanetId;
                Multiplayer.Session.Factories.AddPlanetTimer(packet.PlanetId);
                PlanetData pData = GameMain.gpuiManager.specifyPlanet;
                GameMain.gpuiManager.specifyPlanet = planet;
                
                if (packet.IsRefrom)
                {
                    // Reform whole planet
                    planet.factory.PlanetReformAll(packet.Type, packet.Color, packet.Burry);
                }
                else
                {
                    // Revert whole planet
                    planet.factory.PlanetReformRevert();
                }

                GameMain.gpuiManager.specifyPlanet = pData;
                Multiplayer.Session.Factories.TargetPlanet = NebulaModAPI.PLANET_NONE;
            }
        }
    }
}
