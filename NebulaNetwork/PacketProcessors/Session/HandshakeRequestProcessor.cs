using NebulaModel;
using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Session;
using NebulaModel.Utils;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class HandshakeRequestProcessor : PacketProcessor<HandshakeRequest>
    {
        private PlayerManager playerManager;

        public HandshakeRequestProcessor()
        {
            playerManager = MultiplayerHostSession.Instance?.PlayerManager;
        }

        public override void ProcessPacket(HandshakeRequest packet, NebulaConnection conn)
        {
            if (IsClient) return;

            Player player;
            using (playerManager.GetPendingPlayers(out var pendingPlayers))
            {
                if (!pendingPlayers.TryGetValue(conn, out player))
                {
                    conn.Disconnect(DisconnectionReason.InvalidData);
                    Log.Warn("WARNING: Player tried to handshake without being in the pending list");
                    return;
                }

                pendingPlayers.Remove(conn);
            }


            if (packet.ModVersion != Config.ModVersion)
            {
                conn.Disconnect(DisconnectionReason.ModVersionMismatch, $"{ packet.ModVersion };{ Config.ModVersion }");
                return;
            }

            if (packet.GameVersionSig != GameConfig.gameVersion.sig)
            {
                conn.Disconnect(DisconnectionReason.GameVersionMismatch, $"{ packet.GameVersionSig };{ GameConfig.gameVersion.sig }");
                return;
            }

            if (packet.HasGS2 != (LocalPlayer.GS2_GSSettings != null))
            {
                conn.Disconnect(DisconnectionReason.GalacticScaleMissmatch, "Either the client or the host did or did not have Galactic Scale installed. Please make sure both have it or dont have it.");
                return;
            }

            SimulatedWorld.OnPlayerJoining();

            //TODO: some validation of client cert / generating auth challenge for the client
            // Load old data of the client
            string clientCertHash = CryptoUtils.Hash(packet.ClientCert);
            using (playerManager.GetSavedPlayerData(out var savedPlayerData))
            {
                if (savedPlayerData.TryGetValue(clientCertHash, out var value))
                {
                    player.LoadUserData(value);
                }
                else
                {
                    savedPlayerData.Add(clientCertHash, player.Data);
                }
            }

            // Add the username to the player data
            player.Data.Username = !string.IsNullOrWhiteSpace(packet.Username) ? packet.Username : $"Player {player.Id}";

            // Add the Mecha Color to the player data
            player.Data.MechaColor = packet.MechaColor;

            // Make sure that each player that is currently in the game receives that a new player as join so they can create its RemotePlayerCharacter
            PlayerJoining pdata = new PlayerJoining(player.Data.CreateCopyWithoutMechaData()); // Remove inventory from mecha data
            using (playerManager.GetConnectedPlayers(out var connectedPlayers))
            {
                foreach (var kvp in connectedPlayers)
                {
                    kvp.Value.SendPacket(pdata);
                }
            }

            // Add the new player to the list
            using (playerManager.GetSyncingPlayers(out var syncingPlayers))
            {
                syncingPlayers.Add(conn, player);
            }

            //Add current tech bonuses to the connecting player based on the Host's mecha
            player.Data.Mecha.TechBonuses = new PlayerTechBonuses(GameMain.mainPlayer.mecha);

            var gameDesc = GameMain.data.gameDesc;
            player.SendPacket(new HandshakeResponse(gameDesc.galaxyAlgo, gameDesc.galaxySeed, gameDesc.starCount, gameDesc.resourceMultiplier, player.Data, (LocalPlayer.GS2_GSSettings != null) ? LocalPlayer.GS2GetSettings() : null));
        }
    }
}
