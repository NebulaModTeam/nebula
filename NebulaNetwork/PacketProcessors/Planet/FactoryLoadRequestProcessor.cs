#region

using NebulaAPI;
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
    public override void ProcessPacket(FactoryLoadRequest packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            return;
        }

        var planet = GameMain.galaxy.PlanetById(packet.PlanetID);
        var factory = GameMain.data.GetOrCreateFactory(planet);

        using (var writer = new BinaryUtils.Writer())
        {
            factory.Export(writer.BinaryWriter);
            var data = writer.CloseAndGetBytes();
            Log.Info($"Sent {data.Length} bytes of data for PlanetFactory {planet.name} (ID: {planet.id})");
            conn.SendPacket(new FragmentInfo(data.Length + planet.data.modData.Length));
            conn.SendPacket(new FactoryData(packet.PlanetID, data, planet.data.modData));
        }

        // Add requesting client to connected player, so he can receive following update
        IPlayerManager playerManager = Multiplayer.Session.Network.PlayerManager;
        var player = playerManager.GetSyncingPlayer(conn);
        if (player != null)
        {
            player.Data.LocalPlanetId = packet.PlanetID;
            player.Data.LocalStarId = GameMain.galaxy.PlanetById(packet.PlanetID).star.id;
            using (playerManager.GetConnectedPlayers(out var connectedPlayers))
            {
                connectedPlayers.Add(player.Connection, player);
            }
        }
    }
}
