#region

using NebulaAPI.GameState;
using NebulaAPI.Networking;
using NebulaAPI.Packets;
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
    public StartGameMessageProcessor()
    {
    }

    protected override void ProcessPacket(StartGameMessage packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            if (Multiplayer.Session.IsGameLoaded && !GameMain.isFullscreenPaused)
            {
                var player = Players.Get(conn, EConnectionStatus.Pending);
                if (player is null)
                {
                    Multiplayer.Session.Server.Disconnect(conn, DisconnectionReason.InvalidData);
                    Log.Warn("WARNING: Player tried to enter the game without being in the pending list");
                    return;
                }

                Multiplayer.Session.Server.Players.TryUpgrade(player, EConnectionStatus.Syncing);

                Multiplayer.Session.World.OnPlayerJoining(player.Data.Username);

                // Make sure that each player that is currently in the game receives that a new player as join so they can create its RemotePlayerCharacter
                var pdata = new PlayerJoining((PlayerData)player.Data.CreateCopyWithoutMechaData(),
                    Multiplayer.Session.NumPlayers); // Remove inventory from mecha data

                Server.SendPacket(pdata);

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

            //Request global part of GameData from host
            Log.Info("Requesting global GameData from the server");
            Multiplayer.Session.Network.SendPacket(new GlobalGameDataRequest());
            if (DSPGame.Game != null)
            {
                DSPGame.EndGame();
            }
            // Prepare gameDesc to later start in GlobalGameDataResponseProcessor
            DSPGame.GameDesc = UIRoot.instance.galaxySelect.gameDesc;

            UIRoot.instance.OpenLoadingUI();
            InGamePopup.ShowInfo("Loading".Translate(), "Loading state from server, please wait".Translate(), null);
        }
        else
        {
            InGamePopup.ShowInfo("Unavailable".Translate(), "The host is not ready to let you in, please wait!".Translate(),
                "OK".Translate());
        }
    }
}
