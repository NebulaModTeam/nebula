using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.GameStates;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    public class FactoryLoadRequestProcessor : PacketProcessor<FactoryLoadRequest>
    {
        public override void ProcessPacket(FactoryLoadRequest packet, NebulaConnection conn)
        {
            if (IsClient)
            {
                return;
            }

            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetID);
            PlanetFactory factory = GameMain.data.GetOrCreateFactory(planet);

            using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
            {
                factory.Export(writer.BinaryWriter);
                byte[] data = writer.CloseAndGetBytes();
                Log.Info($"Sent {data.Length} bytes of data for PlanetFactory {planet.name} (ID: {planet.id})");
                conn.SendPacket(new FragmentInfo(data.Length + planet.data.modData.Length));
                conn.SendPacket(new FactoryData(packet.PlanetID, data, planet.data.modData));
            }

            // Add requesting client to connected player, so he can receive following update
            IPlayerManager playerManager = Multiplayer.Session.Network.PlayerManager;
            INebulaPlayer player = playerManager.GetSyncingPlayer(conn);
            if (player != null)
            {
                player.Data.LocalPlanetId = packet.PlanetID;
                player.Data.LocalStarId = GameMain.galaxy.PlanetById(packet.PlanetID).star.id;
                using (playerManager.GetConnectedPlayers(out System.Collections.Generic.Dictionary<INebulaConnection, INebulaPlayer> connectedPlayers))
                {
                    connectedPlayers.Add(player.Connection, player);
                }
            }
        }
    }
}
