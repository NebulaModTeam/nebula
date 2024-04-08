#region

using NebulaAPI;
using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;
using NebulaWorld;
using NebulaWorld.Factory;
using NebulaWorld.Player;
using NebulaWorld.Warning;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory;

[RegisterPacketProcessor]
public class BuildEntityRequestProcessor : PacketProcessor<BuildEntityRequest>
{
    protected override void ProcessPacket(BuildEntityRequest packet, NebulaConnection conn)
    {
        if (IsHost && !Multiplayer.Session.Factories.ContainsPrebuildRequest(packet.PlanetId, packet.PrebuildId))
        {
            // Prebuild has already been removed, so skip it.
            return;
        }

        var planet = GameMain.galaxy.PlanetById(packet.PlanetId);

        // We only execute the code if the client has loaded the factory at least once.
        // Else it will get it once it goes to the planet for the first time. 
        if (planet.factory == null)
        {
            return;
        }
        using (Multiplayer.Session.Factories.IsIncomingRequest.On())
        {
            Multiplayer.Session.Factories.EventFactory = planet.factory;
            Multiplayer.Session.Factories.TargetPlanet = packet.PlanetId;
            Multiplayer.Session.Factories.PacketAuthor = packet.AuthorId;

            if (packet.EntityId != -1 && packet.EntityId != FactoryManager.GetNextEntityId(planet.factory))
            {
                var warningText =
                    string.Format(
                        "(Desync) EntityId mismatch {0} != {1} on planet {2}. Clients should reconnect!".Translate(),
                        packet.EntityId, FactoryManager.GetNextEntityId(planet.factory), planet.displayName);
                Log.WarnInform(warningText);
                WarningManager.DisplayTemporaryWarning(warningText, 5000);
            }

            if (planet.factoryLoaded)
            {
                planet.factory.BuildFinally(GameMain.mainPlayer, packet.PrebuildId, true, true);
            }
            else
            {
                Multiplayer.Session.Factories.AddPlanetTimer(packet.PlanetId);
                // setting specifyPlanet here to avoid accessing a null object (see GPUInstancingManager activePlanet getter)
                var pData = GameMain.gpuiManager.specifyPlanet;
                GameMain.gpuiManager.specifyPlanet = GameMain.galaxy.PlanetById(packet.PlanetId);
                // Flatten the terrain for remote planet build by other players
                planet.factory.BuildFinally(GameMain.mainPlayer, packet.PrebuildId, true, true);
                GameMain.gpuiManager.specifyPlanet = pData;
            }

            Multiplayer.Session.Factories.EventFactory = null;
            Multiplayer.Session.Factories.PacketAuthor = NebulaModAPI.AUTHOR_NONE;
            Multiplayer.Session.Factories.TargetPlanet = NebulaModAPI.PLANET_NONE;
        }
    }
}
