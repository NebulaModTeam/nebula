#region

using NebulaAPI;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Foundation;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Foundation;

[RegisterPacketProcessor]
internal class PlanetReformProcessor : PacketProcessor<PlanetReformPacket>
{
    protected override void ProcessPacket(PlanetReformPacket packet, NebulaConnection conn)
    {
        using (Multiplayer.Session.Planets.IsIncomingRequest.On())
        {
            var planet = GameMain.galaxy.PlanetById(packet.PlanetId);
            if (planet?.factory == null)
            {
                return;
            }
            Multiplayer.Session.Factories.TargetPlanet = packet.PlanetId;
            Multiplayer.Session.Factories.AddPlanetTimer(packet.PlanetId);
            var pData = GameMain.gpuiManager.specifyPlanet;
            GameMain.gpuiManager.specifyPlanet = planet;

            if (packet.IsReform)
            {
                // Reform whole planet
                planet.factory.PlanetReformAll(packet.Type, packet.Color, packet.Bury);
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
