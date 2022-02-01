using NebulaAPI;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Session;
using NebulaModel.Utils;
using NebulaWorld;
using System.Collections.Generic;

namespace NebulaNetwork.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class LobbyRequestProcessor: PacketProcessor<LobbyRequest>
    {
        private readonly IPlayerManager playerManager;
        public LobbyRequestProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(LobbyRequest packet, NebulaConnection conn)
        {
            if (IsClient)
            {
                return;
            }

            INebulaPlayer player;
            using (playerManager.GetPendingPlayers(out Dictionary<INebulaConnection, INebulaPlayer> pendingPlayers))
            {
                if (!pendingPlayers.TryGetValue(conn, out player))
                {
                    conn.Disconnect(DisconnectionReason.InvalidData);
                    Log.Warn("WARNING: Player tried to enter lobby without being in the pending list");
                    return;
                }

                pendingPlayers.Remove(conn);
            }

            Dictionary<string, string> clientMods = new Dictionary<string, string>();

            using (BinaryUtils.Reader reader = new BinaryUtils.Reader(packet.ModsVersion))
            {
                for (int i = 0; i < packet.ModsCount; i++)
                {
                    string guid = reader.BinaryReader.ReadString();
                    string version = reader.BinaryReader.ReadString();

                    if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(guid))
                    {
                        conn.Disconnect(DisconnectionReason.ModIsMissingOnServer, guid);
                        return;
                    }

                    clientMods.Add(guid, version);
                }
            }

            foreach (KeyValuePair<string, BepInEx.PluginInfo> pluginInfo in BepInEx.Bootstrap.Chainloader.PluginInfos)
            {
                if (pluginInfo.Value.Instance is IMultiplayerMod mod)
                {
                    if (!clientMods.ContainsKey(pluginInfo.Key))
                    {
                        conn.Disconnect(DisconnectionReason.ModIsMissing, pluginInfo.Key);
                        return;
                    }

                    string version = clientMods[pluginInfo.Key];

                    if (mod.CheckVersion(mod.Version, version))
                    {
                        continue;
                    }

                    conn.Disconnect(DisconnectionReason.ModVersionMismatch, $"{pluginInfo.Key};{version};{mod.Version}");
                    return;
                }
            }

            if (packet.GameVersionSig != GameConfig.gameVersion.sig)
            {
                conn.Disconnect(DisconnectionReason.GameVersionMismatch, $"{ packet.GameVersionSig };{ GameConfig.gameVersion.sig }");
                return;
            }

            bool isNewUser = false;

            //TODO: some validation of client cert / generating auth challenge for the client
            // Load old data of the client
            string clientCertHash = CryptoUtils.Hash(packet.ClientCert);
            using (playerManager.GetSavedPlayerData(out Dictionary<string, IPlayerData> savedPlayerData))
            {
                if (savedPlayerData.TryGetValue(clientCertHash, out IPlayerData value))
                {
                    player.LoadUserData(value);
                }
                else
                {
                    // store player data once he fully loaded into the game (SyncCompleteProcessor)
                    isNewUser = true;
                }
            }

            // Add the username to the player data
            player.Data.Username = !string.IsNullOrWhiteSpace(packet.Username) ? packet.Username : $"Player {player.Id}";

            // Add the Mecha Color to the player data
            player.Data.MechaColors = packet.MechaColors;

            // Add the new player to the list
            using (playerManager.GetSyncingPlayers(out Dictionary<INebulaConnection, INebulaPlayer> syncingPlayers))
            {
                syncingPlayers.Add(conn, player);
            }

            // if user is known and host is ingame dont put him into lobby but let him join the game
            if (!isNewUser && Multiplayer.Session.IsGameLoaded)
            {
                Multiplayer.Session.World.OnPlayerJoining(player.Data.Username);

                // Make sure that each player that is currently in the game receives that a new player as join so they can create its RemotePlayerCharacter
                PlayerJoining pdata = new PlayerJoining((PlayerData)player.Data.CreateCopyWithoutMechaData()); // Remove inventory from mecha data
                using (playerManager.GetConnectedPlayers(out Dictionary<INebulaConnection, INebulaPlayer> connectedPlayers))
                {
                    foreach (KeyValuePair<INebulaConnection, INebulaPlayer> kvp in connectedPlayers)
                    {
                        kvp.Value.SendPacket(pdata);
                    }
                }

                //Add current tech bonuses to the connecting player based on the Host's mecha
                ((MechaData)player.Data.Mecha).TechBonuses = new PlayerTechBonuses(GameMain.mainPlayer.mecha);

                using (BinaryUtils.Writer p = new BinaryUtils.Writer())
                {
                    int count = 0;
                    foreach (KeyValuePair<string, BepInEx.PluginInfo> pluginInfo in BepInEx.Bootstrap.Chainloader.PluginInfos)
                    {
                        if (pluginInfo.Value.Instance is IMultiplayerModWithSettings mod)
                        {
                            p.BinaryWriter.Write(pluginInfo.Key);
                            mod.Export(p.BinaryWriter);
                            count++;
                        }
                    }

                    GameDesc gameDesc = GameMain.data.gameDesc;
                    player.SendPacket(new HandshakeResponse(gameDesc.galaxyAlgo, gameDesc.galaxySeed, gameDesc.starCount, gameDesc.resourceMultiplier, isNewUser, (PlayerData)player.Data, p.CloseAndGetBytes(), count));
                }
            }
            else
            {
                UIGalaxySelect galaxySelect = UIRoot.instance.galaxySelect;

                using (BinaryUtils.Writer p = new BinaryUtils.Writer())
                {
                    int count = 0;
                    foreach (KeyValuePair<string, BepInEx.PluginInfo> pluginInfo in BepInEx.Bootstrap.Chainloader.PluginInfos)
                    {
                        if (pluginInfo.Value.Instance is IMultiplayerModWithSettings mod)
                        {
                            p.BinaryWriter.Write(pluginInfo.Key);
                            mod.Export(p.BinaryWriter);
                            count++;
                        }
                    }

                    if (Multiplayer.Session.IsGameLoaded)
                    {
                        GameDesc gameDesc = GameMain.data.gameDesc;
                        player.SendPacket(new LobbyResponse(gameDesc.galaxyAlgo, gameDesc.galaxySeed, gameDesc.starCount, gameDesc.resourceMultiplier, p.CloseAndGetBytes(), count));
                    }
                    else
                    {
                        player.SendPacket(new LobbyResponse(galaxySelect.gameDesc.galaxyAlgo, galaxySelect.gameDesc.galaxySeed, galaxySelect.gameDesc.starCount, galaxySelect.gameDesc.resourceMultiplier, p.CloseAndGetBytes(), count));
                    }
                }
            }
        }
    }
}
