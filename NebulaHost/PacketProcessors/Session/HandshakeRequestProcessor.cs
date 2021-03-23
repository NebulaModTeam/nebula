using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Session;
using NebulaWorld;
using System.Linq;

namespace NebulaHost.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class HandshakeRequestProcessor : IPacketProcessor<HandshakeRequest>
    {
        private PlayerManager playerManager;

        public HandshakeRequestProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(HandshakeRequest packet, NebulaConnection conn)
        {
            Player player;
            if (!playerManager.PendingPlayers.TryGetValue(conn, out player))
            {
                conn.Disconnect();
                Log.Warn("WARNING: Player tried to handshake without being in the pending list");
                return;
            }

            playerManager.PendingPlayers.Remove(conn);

            if (packet.ProtocolVersion != 0) //TODO: Maybe have a shared constants file somewhere for this
            {
                conn.Disconnect();
            }

            //
            GameMain.Pause();
            InGamePopup.ShowInfo("Loading", "Player joining the game, please wait", null);

            // Make sure that each player that is currently in the game receives that a new player as join so they can create its RemotePlayerCharacter
            foreach (Player activePlayer in playerManager.GetConnectedPlayers())
            {
                activePlayer.SendPacket(new PlayerJoining(player.Data));
            }

            // Add the new player to the list
            playerManager.SyncingPlayers.Add(conn, player);

            // TODO: This should be our actual GameDesc and not an hardcoded value.
            var inGamePlayersIds = playerManager.GetAllPlayerIdsIncludingHost();

            var gameDesc = GameMain.data.gameDesc;
            player.SendPacket(new HandshakeResponse(gameDesc.galaxyAlgo, gameDesc.galaxySeed, gameDesc.starCount, gameDesc.resourceMultiplier, player.Data, inGamePlayersIds.ToArray()));

            SimulatedWorld.SpawnRemotePlayerModel(player.Data);
        }
    }
}
