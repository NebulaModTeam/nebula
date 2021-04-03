using NebulaModel.Attributes;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Session;
using NebulaModel.Utils;
using NebulaWorld;

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
                conn.Disconnect(DisconnectionReason.InvalidData);
                Log.Warn("WARNING: Player tried to handshake without being in the pending list");
                return;
            }

            playerManager.PendingPlayers.Remove(conn);

            if (packet.ProtocolVersion != 0) //TODO: Maybe have a shared constants file somewhere for this
            {
                conn.Disconnect(DisconnectionReason.ProtocolError);
                return;
            }

            SimulatedWorld.OnPlayerJoining();

            //TODO: some validation of client cert / generating auth challenge for the client
            // Load old data of the client
            string clientCertHash = CryptoUtils.Hash(packet.ClientCert);
            if (playerManager.SavedPlayerData.ContainsKey(clientCertHash))
            {
                player.LoadUserData(playerManager.SavedPlayerData[clientCertHash]);
            }
            else
            {
                playerManager.SavedPlayerData.Add(clientCertHash, player.Data);
            }

            // Make sure that each player that is currently in the game receives that a new player as join so they can create its RemotePlayerCharacter
            PlayerData pdata = player.Data.CreateCopyWithoutMechaData(); // Remove inventory from mecha data
            foreach (Player activePlayer in playerManager.GetConnectedPlayers())
            {
                activePlayer.SendPacket(new PlayerJoining(pdata));
            }

            // Add the new player to the list
            playerManager.SyncingPlayers.Add(conn, player);

            //Add current tech bonuses to the connecting player based on the Host's mecha
            player.Data.Mecha.TechBonuses = new PlayerTechBonuses(GameMain.mainPlayer.mecha);

            var gameDesc = GameMain.data.gameDesc;
            player.SendPacket(new HandshakeResponse(gameDesc.galaxyAlgo, gameDesc.galaxySeed, gameDesc.starCount, gameDesc.resourceMultiplier, player.Data));
        }
    }
}
