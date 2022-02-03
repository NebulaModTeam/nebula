using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Session;
using NebulaModel.Packets.Universe;
using NebulaModel.Utils;
using NebulaWorld;
using System.Collections.Generic;

namespace NebulaNetwork.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class SyncCompleteProcessor : PacketProcessor<SyncComplete>
    {
        private readonly IPlayerManager playerManager;

        public SyncCompleteProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(SyncComplete packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                INebulaPlayer player = playerManager.GetSyncingPlayer(conn);
                if (player == null)
                {
                    Log.Warn("Received a SyncComplete packet, but no player is joining.");
                    return;
                }

                // store the player now, not when he enters the lobby. that would cause weird teleportations when clients reenter the lobby without ever having loaded into the game
                string clientCertHash = CryptoUtils.Hash(packet.ClientCert);
                using (playerManager.GetSavedPlayerData(out Dictionary<string, IPlayerData> savedPlayerData))
                {
                    if (!savedPlayerData.TryGetValue(clientCertHash, out IPlayerData value))
                    {
                        savedPlayerData.Add(clientCertHash, player.Data);
                    }
                }

                // Should these be locked together?

                int syncingCount;
                using (playerManager.GetSyncingPlayers(out System.Collections.Generic.Dictionary<INebulaConnection, INebulaPlayer> syncingPlayers))
                {
                    bool removed = syncingPlayers.Remove(player.Connection);
                    syncingCount = syncingPlayers.Count;
                }

                using (playerManager.GetConnectedPlayers(out System.Collections.Generic.Dictionary<INebulaConnection, INebulaPlayer> connectedPlayers))
                {
                    connectedPlayers.Add(player.Connection, player);
                }

                // Load overriden Planet and Star names
                foreach (StarData s in GameMain.galaxy.stars)
                {
                    if (!string.IsNullOrEmpty(s.overrideName))
                    {
                        player.SendPacket(new NameInputPacket(s.overrideName, s.id, NebulaModAPI.PLANET_NONE, Multiplayer.Session.LocalPlayer.Id));
                    }

                    foreach (PlanetData p in s.planets)
                    {
                        if (!string.IsNullOrEmpty(p.overrideName))
                        {
                            player.SendPacket(new NameInputPacket(p.overrideName, NebulaModAPI.STAR_NONE, p.id, Multiplayer.Session.LocalPlayer.Id));
                        }
                    }
                }

                // Since the player is now connected, we can safely spawn his player model
                Multiplayer.Session.World.SpawnRemotePlayerModel(player.Data);

                if (syncingCount == 0)
                {
                    IPlayerData[] inGamePlayersDatas = playerManager.GetAllPlayerDataIncludingHost();
                    playerManager.SendPacketToAllPlayers(new SyncComplete(inGamePlayersDatas));

                    // Since the host is always in the game he could already have changed his mecha armor, so send it to the new player.
                    using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
                    {
                        GameMain.mainPlayer.mecha.appearance.Export(writer.BinaryWriter);
                        player.SendPacket(new PlayerMechaArmor(Multiplayer.Session.LocalPlayer.Id, writer.CloseAndGetBytes()));
                    }

                    // if the client had used a custom armor we should have saved a copy of it, so send it back
                    if(player.Data.Appearance != null)
                    {
                        using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
                        {
                            player.Data.Appearance.Export(writer.BinaryWriter);
                            playerManager.SendPacketToAllPlayers(new PlayerMechaArmor(player.Id, writer.CloseAndGetBytes()));
                        }

                        // and load custom appearance on host side too
                        // this is the code from PlayerMechaArmonrProcessor
                        using (Multiplayer.Session.World.GetRemotePlayersModels(out Dictionary<ushort, RemotePlayerModel> remotePlayersModels))
                        {
                            if(remotePlayersModels.TryGetValue(player.Id, out RemotePlayerModel playerModel))
                            {
                                if(playerModel.MechaInstance.appearance == null)
                                {
                                    playerModel.MechaInstance.appearance = new MechaAppearance();
                                    playerModel.MechaInstance.appearance.Init();
                                }
                                player.Data.Appearance.CopyTo(playerModel.MechaInstance.appearance);
                                playerModel.PlayerInstance.mechaArmorModel.RefreshAllPartObjects();
                                playerModel.PlayerInstance.mechaArmorModel.RefreshAllBoneObjects();
                                playerModel.MechaInstance.appearance.NotifyAllEvents();
                                playerModel.PlayerInstance.mechaArmorModel._Init(playerModel.PlayerInstance);
                                playerModel.PlayerInstance.mechaArmorModel._OnOpen();
                            }
                        }
                    }

                    Multiplayer.Session.World.OnAllPlayersSyncCompleted();
                }
            }
            else // IsClient
            {
                // Everyone is now connected, we can safely spawn the player model of all the other players that are currently connected
                foreach (NebulaModel.DataStructures.PlayerData playerData in packet.AllPlayers)
                {
                    if (playerData.PlayerId != Multiplayer.Session.LocalPlayer.Id)
                    {
                        Multiplayer.Session.World.SpawnRemotePlayerModel(playerData);
                    }
                }

                Multiplayer.Session.World.OnAllPlayersSyncCompleted();
            }
        }
    }
}
