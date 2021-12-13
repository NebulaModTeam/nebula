using NebulaAPI;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Session;
using NebulaWorld;
using System.Collections.Generic;

namespace NebulaNetwork.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    class StartGameMessageProcessor: PacketProcessor<StartGameMessage>
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
                INebulaPlayer player;
                using (playerManager.GetSyncingPlayers(out Dictionary<INebulaConnection, INebulaPlayer> syncingPlayers))
                {
                    if (!syncingPlayers.TryGetValue(conn, out player))
                    {
                        conn.Disconnect(DisconnectionReason.InvalidData);
                        syncingPlayers.Remove(conn);
                        Log.Warn("WARNING: Player tried to start a game without being in the syncing list");
                        return;
                    }
                }

                if(Multiplayer.Session.IsGameLoaded && !GameMain.isFullscreenPaused)
                {
                    Multiplayer.Session.World.OnPlayerJoining();

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

                    conn.SendPacket(new StartGameMessage(true, (PlayerData)player.Data));
                }
                else
                {
                    conn.SendPacket(new StartGameMessage(false, null));
                }
            }
            else if(packet.IsAllowedToStart)
            {
                ((LocalPlayer)Multiplayer.Session.LocalPlayer).SetPlayerData(packet.LocalPlayerData, true);

                GameDesc gameDesc = UIRoot.instance.galaxySelect.gameDesc;
                gameDesc.SetForNewGame(gameDesc.galaxyAlgo, gameDesc.galaxySeed, gameDesc.starCount, 1, gameDesc.resourceMultiplier);
                DSPGame.StartGameSkipPrologue(gameDesc);

                InGamePopup.ShowInfo("Loading", "Loading state from server, please wait", null);
            }
            else
            {
                InGamePopup.ShowInfo("Please Wait", "The host is not ready to let you in, please wait!", "Okay");
            }
        }
    }
}
