#region

using NebulaAPI;
using NebulaModel;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Session;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Session;

[RegisterPacketProcessor]
internal class StartGameMessageProcessor : PacketProcessor<StartGameMessage>
{
    private readonly IPlayerManager playerManager;

    public StartGameMessageProcessor()
    {
        playerManager = Multiplayer.Session.Network.PlayerManager;
    }

    public override void ProcessPacket(StartGameMessage packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            if (Multiplayer.Session.IsGameLoaded && !GameMain.isFullscreenPaused)
            {
                INebulaPlayer player;
                using (playerManager.GetPendingPlayers(out var pendingPlayers))
                {
                    if (!pendingPlayers.TryGetValue(conn, out player))
                    {
                        conn.Disconnect(DisconnectionReason.InvalidData);
                        Log.Warn("WARNING: Player tried to enter the game without being in the pending list");
                        return;
                    }

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

                conn.SendPacket(new StartGameMessage(true, (PlayerData)player.Data, Config.Options.SyncSoil));
            }
            else
            {
                conn.SendPacket(new StartGameMessage(false, null, false));
            }
        }
        else if (packet.IsAllowedToStart)
        {
            // overwrite local setting with host setting, but dont save it as its a temp setting for this session
            Config.Options.SyncSoil = packet.SyncSoil;

            ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost = false;
            ((LocalPlayer)Multiplayer.Session.LocalPlayer).SetPlayerData(packet.LocalPlayerData, true);

            UIRoot.instance.uiGame.planetDetail.gameObject.SetActive(false);
            Multiplayer.Session.IsInLobby = false;
            Multiplayer.ShouldReturnToJoinMenu = false;

            DSPGame.StartGameSkipPrologue(UIRoot.instance.galaxySelect.gameDesc);

            InGamePopup.ShowInfo("Loading".Translate(), "Loading state from server, please wait".Translate(), null);
        }
        else
        {
            InGamePopup.ShowInfo("Unavailable".Translate(), "The host is not ready to let you in, please wait!".Translate(),
                "OK".Translate());
        }
    }
}
