#region

using System.Collections.Generic;
using BepInEx.Bootstrap;
using NebulaAPI;
using NebulaModel;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Session;
using NebulaModel.Packets.Universe;
using NebulaModel.Utils;
using NebulaWorld;
using NebulaWorld.SocialIntegration;

#endregion

namespace NebulaNetwork.PacketProcessors.Session;

[RegisterPacketProcessor]
public class LobbyRequestProcessor : PacketProcessor<LobbyRequest>
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
        using (playerManager.GetPendingPlayers(out var pendingPlayers))
        {
            if (!pendingPlayers.TryGetValue(conn, out player))
            {
                conn.Disconnect(DisconnectionReason.InvalidData);
                Log.Warn("WARNING: Player tried to enter lobby without being in the pending list");
                return;
            }

            if (GameMain.isFullscreenPaused)
            {
                Log.Warn("Reject connection because server is still loading");
                conn.Disconnect(DisconnectionReason.HostStillLoading);
                pendingPlayers.Remove(conn);
                return;
            }

            if (!ModsVersionCheck(packet, out var disconnectionReason, out var reasonString))
            {
                Log.Warn("Reject connection because mods mismatch");
                conn.Disconnect(disconnectionReason, reasonString);
                pendingPlayers.Remove(conn);
                return;
            }
        }

        var isNewUser = false;

        //TODO: some validation of client cert / generating auth challenge for the client
        // Load old data of the client
        var clientCertHash = CryptoUtils.Hash(packet.ClientCert);
        using (playerManager.GetSavedPlayerData(out var savedPlayerData))
        {
            if (savedPlayerData.TryGetValue(clientCertHash, out var value))
            {
                var playerData = value;
                using (playerManager.GetConnectedPlayers(out var connectedPlayers))
                {
                    foreach (var connectedPlayer in connectedPlayers.Values)
                    {
                        if (connectedPlayer.Data == playerData)
                        {
                            playerData = value.CreateCopyWithoutMechaData();
                            Log.Warn($"Copy playerData for duplicated player{playerData.PlayerId} {playerData.Username}");
                        }
                    }
                }
                player.LoadUserData(playerData);
            }
            else
            {
                // store player data once he fully loaded into the game (SyncCompleteProcessor)
                isNewUser = true;
            }
        }

        // Add the username to the player data
        player.Data.Username = !string.IsNullOrWhiteSpace(packet.Username) ? packet.Username : $"Player {player.Id}";

        Multiplayer.Session.NumPlayers += 1;
        DiscordManager.UpdateRichPresence();

        // if user is known and host is ingame dont put him into lobby but let him join the game
        if (!isNewUser && Multiplayer.Session.IsGameLoaded)
        {
            // Remove the new player from pending list
            using (playerManager.GetPendingPlayers(out var pendingPlayers))
            {
                pendingPlayers.Remove(conn);
            }

            // Add the new player to the list
            using (playerManager.GetSyncingPlayers(out var syncingPlayers))
            {
                syncingPlayers.Add(conn, player);
            }

            Multiplayer.Session.World.OnPlayerJoining(player.Data.Username);

            // Make sure that each player that is currently in the game receives that a new player as join so they can create its RemotePlayerCharacter
            var pdata = new PlayerJoining((PlayerData)player.Data.CreateCopyWithoutMechaData(),
                Multiplayer.Session.NumPlayers); // Remove inventory from mecha data
            using (playerManager.GetConnectedPlayers(out var connectedPlayers))
            {
                foreach (var kvp in connectedPlayers)
                {
                    kvp.Value.SendPacket(pdata);
                }
            }

            //Add current tech bonuses to the connecting player based on the Host's mecha
            ((MechaData)player.Data.Mecha).TechBonuses = new PlayerTechBonuses(GameMain.mainPlayer.mecha);

            using (var p = new BinaryUtils.Writer())
            {
                var count = 0;
                foreach (var pluginInfo in Chainloader.PluginInfos)
                {
                    if (pluginInfo.Value.Instance is IMultiplayerModWithSettings mod)
                    {
                        p.BinaryWriter.Write(pluginInfo.Key);
                        mod.Export(p.BinaryWriter);
                        count++;
                    }
                }

                var gameDesc = GameMain.data.gameDesc;
                player.SendPacket(new HandshakeResponse(in gameDesc, isNewUser, (PlayerData)player.Data, p.CloseAndGetBytes(),
                    count, Config.Options.SyncSoil, Multiplayer.Session.NumPlayers, DiscordManager.GetPartyId()));
            }
        }
        else
        {
            var gameDesc = Multiplayer.Session.IsGameLoaded ? GameMain.data.gameDesc : UIRoot.instance.galaxySelect.gameDesc;

            using (var p = new BinaryUtils.Writer())
            {
                var count = 0;
                foreach (var pluginInfo in Chainloader.PluginInfos)
                {
                    if (pluginInfo.Value.Instance is IMultiplayerModWithSettings mod)
                    {
                        p.BinaryWriter.Write(pluginInfo.Key);
                        mod.Export(p.BinaryWriter);
                        count++;
                    }
                }

                player.SendPacket(new LobbyResponse(in gameDesc, p.CloseAndGetBytes(), count, Multiplayer.Session.NumPlayers,
                    DiscordManager.GetPartyId()));
            }

            // Send overriden Planet and Star names
            player.SendPacket(new NameInputPacket(GameMain.galaxy, Multiplayer.Session.LocalPlayer.Id));
        }
    }


    private bool ModsVersionCheck(in LobbyRequest packet, out DisconnectionReason reason, out string reasonString)
    {
        reason = DisconnectionReason.Normal;
        reasonString = null;
        var clientMods = new Dictionary<string, string>();

        using (var reader = new BinaryUtils.Reader(packet.ModsVersion))
        {
            for (var i = 0; i < packet.ModsCount; i++)
            {
                var guid = reader.BinaryReader.ReadString();
                var version = reader.BinaryReader.ReadString();

                if (!Chainloader.PluginInfos.ContainsKey(guid))
                {
                    reason = DisconnectionReason.ModIsMissingOnServer;
                    reasonString = guid;
                    return false;
                }

                clientMods.Add(guid, version);
            }
        }

        foreach (var pluginInfo in Chainloader.PluginInfos)
        {
            if (pluginInfo.Value.Instance is IMultiplayerMod mod)
            {
                if (!clientMods.ContainsKey(pluginInfo.Key))
                {
                    reason = DisconnectionReason.ModIsMissing;
                    reasonString = pluginInfo.Key;
                    return false;
                }

                var version = clientMods[pluginInfo.Key];

                if (mod.CheckVersion(mod.Version, version))
                {
                    continue;
                }

                reason = DisconnectionReason.ModVersionMismatch;
                reasonString = $"{pluginInfo.Key};{version};{mod.Version}";
                return false;
            }
            foreach (var dependency in pluginInfo.Value.Dependencies)
            {
                if (dependency.DependencyGUID == NebulaModAPI.API_GUID)
                {
                    var hostVersion = pluginInfo.Value.Metadata.Version.ToString();
                    if (!clientMods.ContainsKey(pluginInfo.Key))
                    {
                        reason = DisconnectionReason.ModIsMissing;
                        reasonString = pluginInfo.Key;
                        return false;
                    }
                    if (clientMods[pluginInfo.Key] != hostVersion)
                    {
                        reason = DisconnectionReason.ModVersionMismatch;
                        reasonString = $"{pluginInfo.Key};{clientMods[pluginInfo.Key]};{hostVersion}";
                        return false;
                    }
                }
            }
        }

        if (packet.GameVersionSig != GameConfig.gameVersion.sig)
        {
            reason = DisconnectionReason.GameVersionMismatch;
            reasonString = $"{packet.GameVersionSig};{GameConfig.gameVersion.sig}";
            return false;
        }

        return true;
    }
}
