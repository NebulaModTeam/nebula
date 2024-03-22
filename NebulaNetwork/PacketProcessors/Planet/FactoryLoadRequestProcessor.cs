#region

using NebulaAPI.Networking;
using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameStates;
using NebulaModel.Packets.Planet;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Planet;

[RegisterPacketProcessor]
public class FactoryLoadRequestProcessor : PacketProcessor<FactoryLoadRequest>
{
    protected override void ProcessPacket(FactoryLoadRequest packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            return;
        }

        var planet = GameMain.galaxy.PlanetById(packet.PlanetID);
        var factory = GameMain.data.GetOrCreateFactory(planet);
        GameMain.galaxy.astrosFactory[planet.id] = factory;
        OnPlanetFactoryLoad(planet);

        using (var writer = new BinaryUtils.Writer())
        {
            factory.Export(writer.BinaryWriter.BaseStream, writer.BinaryWriter);
            var data = writer.CloseAndGetBytes();
            Log.Info($"Sent {data.Length} bytes of data for PlanetFactory {planet.name} (ID: {planet.id})");
            conn.SendPacket(new FragmentInfo(data.Length + planet.data.modData.Length));
            conn.SendPacket(new FactoryData(packet.PlanetID, data, planet.data.modData));
        }

        // Update syncing player data (Connected player will be update by movement packets)
        var player = Multiplayer.Session.Server.Players.Get(conn, EConnectionStatus.Syncing);
        if (player != null)
        {
            player.Data.LocalPlanetId = packet.PlanetID;
            player.Data.LocalStarId = GameMain.galaxy.PlanetById(packet.PlanetID).star.id;
        }
    }

    static void OnPlanetFactoryLoad(PlanetData planet)
    {
        //Realize planet bases before sending the factory data
        var spaceSector = GameMain.data.spaceSector;
        var enemyDFHiveSystem = spaceSector.dfHives[planet.star.index];
        while (enemyDFHiveSystem != null)
        {
            for (var i = 0; i < enemyDFHiveSystem.relays.cursor; i++)
            {
                var dfrelayComponent = enemyDFHiveSystem.relays[i];
                if (dfrelayComponent?.targetAstroId == planet.astroId && dfrelayComponent.baseState == 1 && dfrelayComponent.stage == 2)
                {
                    dfrelayComponent.RealizePlanetBase(spaceSector);
                }
            }
            enemyDFHiveSystem = enemyDFHiveSystem.nextSibling;
        }

        // Set entities (building) on the planet to full health
        // Note: Try to sync the current value in the future
        var astroId = planet.astroId;
        var combatStatCursor = spaceSector.skillSystem.combatStats.cursor;
        var buffer = spaceSector.skillSystem.combatStats.buffer;
        for (var id = 1; id < combatStatCursor; id++)
        {
            if (buffer[id].id == id && buffer[id].astroId == astroId && buffer[id].objectType == 0)
            {
                buffer[id].HandleFullHp(GameMain.data, spaceSector.skillSystem);
            }
        }

    }
}
