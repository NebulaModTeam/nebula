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
}
