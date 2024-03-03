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
public class UpgradeEntityRequestProcessor : PacketProcessor<UpgradeEntityRequest>
{
    protected override void ProcessPacket(UpgradeEntityRequest packet, NebulaConnection conn)
    {
        using (Multiplayer.Session.Factories.IsIncomingRequest.On())
        {
            var planet = GameMain.galaxy.PlanetById(packet.PlanetId);

            // We only execute the code if the client has loaded the factory at least once.
            // Else they will get it once they go to the planet for the first time. 
            if (planet?.factory == null)
            {
                return;
            }

            Multiplayer.Session.Factories.TargetPlanet = packet.PlanetId;
            Multiplayer.Session.Factories.PacketAuthor = packet.AuthorId;

            Multiplayer.Session.Factories.AddPlanetTimer(packet.PlanetId);
            var itemProto = LDB.items.Select(packet.UpgradeProtoId);

            // setting specifyPlanet here to avoid accessing a null object (see GPUInstancingManager activePlanet getter)
            var pData = GameMain.gpuiManager.specifyPlanet;

            GameMain.gpuiManager.specifyPlanet = planet;
            planet.factory.UpgradeFinally(GameMain.mainPlayer, packet.ObjId, itemProto);
            GameMain.gpuiManager.specifyPlanet = pData;

            Multiplayer.Session.Factories.TargetPlanet = NebulaModAPI.PLANET_NONE;
            Multiplayer.Session.Factories.PacketAuthor = NebulaModAPI.AUTHOR_NONE;
        }
    }
}
