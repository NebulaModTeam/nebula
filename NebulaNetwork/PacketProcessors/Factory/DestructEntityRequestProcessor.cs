#region

using NebulaAPI;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory;

[RegisterPacketProcessor]
public class DestructEntityRequestProcessor : PacketProcessor<DestructEntityRequest>
{
    protected override void ProcessPacket(DestructEntityRequest packet, NebulaConnection conn)
    {
        using (Multiplayer.Session.Factories.IsIncomingRequest.On())
        {
            var planet = GameMain.galaxy.PlanetById(packet.PlanetId);
            var pab = GameMain.mainPlayer.controller != null ? GameMain.mainPlayer.controller.actionBuild : null;

            // We only execute the code if the client has loaded the factory at least once.
            // Else they will get it once they go to the planet for the first time. 
            if (planet?.factory == null || pab == null)
            {
                return;
            }

            Multiplayer.Session.Factories.TargetPlanet = packet.PlanetId;
            Multiplayer.Session.Factories.PacketAuthor = packet.AuthorId;
            var tmpFactory = pab.factory;
            pab.factory = planet.factory;
            pab.noneTool.factory = planet.factory;

            Multiplayer.Session.Factories.AddPlanetTimer(packet.PlanetId);

            // setting specifyPlanet here to avoid accessing a null object (see GPUInstancingManager activePlanet getter)
            var pData = GameMain.gpuiManager.specifyPlanet;

            GameMain.gpuiManager.specifyPlanet = GameMain.galaxy.PlanetById(packet.PlanetId);
            pab.DoDismantleObject(packet.ObjId);
            GameMain.gpuiManager.specifyPlanet = pData;

            pab.factory = tmpFactory;
            pab.noneTool.factory = tmpFactory;
            Multiplayer.Session.Factories.TargetPlanet = NebulaModAPI.PLANET_NONE;
            Multiplayer.Session.Factories.PacketAuthor = NebulaModAPI.AUTHOR_NONE;
        }
    }
}
