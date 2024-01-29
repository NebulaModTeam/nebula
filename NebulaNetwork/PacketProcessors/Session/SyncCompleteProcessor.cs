#region

using NebulaAPI.GameState;
using NebulaAPI.Networking;
using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Session;
using NebulaModel.Utils;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Session;

[RegisterPacketProcessor]
public class SyncCompleteProcessor : PacketProcessor<SyncComplete>
{
    protected override void ProcessPacket(SyncComplete packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            ServerSyncComplete(packet, conn);
        }
        else // IsClient
        {
            // Everyone is now connected, we can safely spawn the player model of all the other players that are currently connected
            foreach (var playerData in packet.AllPlayers)
            {
                if (playerData.PlayerId != Multiplayer.Session.LocalPlayer.Id)
                {
                    Multiplayer.Session.World.SpawnRemotePlayerModel(playerData);
                }
            }

            Multiplayer.Session.World.OnAllPlayersSyncCompleted();
        }
    }

    private void ServerSyncComplete(SyncComplete packet, NebulaConnection conn)
    {
        var player = Players.Get(conn, EConnectionStatus.Syncing);
        if (player == null)
        {
            Log.Warn("Received a SyncComplete packet, but no player is joining.");
            Multiplayer.Session.World.OnAllPlayersSyncCompleted();
            return;
        }

        // store the player now, not when he enters the lobby. that would cause weird teleportations when clients reenter the lobby without ever having loaded into the game
        var clientCertHash = CryptoUtils.Hash(packet.ClientCert);

        SaveManager.TryAdd(clientCertHash, player.Data);

        Multiplayer.Session.Server.Players.TryUpgrade(player, EConnectionStatus.Connected);

        // Since the player is now connected, we can safely spawn his player model
        SimulatedWorld.OnPlayerJoinedGame(player);

        var syncingCount = Multiplayer.Session.Server.Players.Syncing.Count;
        if (syncingCount != 0)
        {
            return;
        }

        var inGamePlayersDatas = Multiplayer.Session.Server.Players.GetAllPlayerData();
        Server.SendPacket(new SyncComplete(inGamePlayersDatas));

        // Since the host is always in the game he could already have changed his mecha armor, so send it to the new player.
        using (var writer = new BinaryUtils.Writer())
        {
            GameMain.mainPlayer.mecha.appearance.Export(writer.BinaryWriter);
            player.SendPacket(new PlayerMechaArmor(Multiplayer.Session.LocalPlayer.Id, writer.CloseAndGetBytes()));
        }

        // if the client had used a custom armor we should have saved a copy of it, so send it back
        if (player.Data.Appearance != null)
        {
            using (var writer = new BinaryUtils.Writer())
            {
                player.Data.Appearance.Export(writer.BinaryWriter);
                Server.SendPacket(new PlayerMechaArmor(player.Id, writer.CloseAndGetBytes()));
            }

            // and load custom appearance on host side too
            // this is the code from PlayerMechaArmonrProcessor
            using (Multiplayer.Session.World.GetRemotePlayersModels(
                       out var remotePlayersModels))
            {
                if (remotePlayersModels.TryGetValue(player.Id, out var playerModel))
                {
                    if (playerModel.MechaInstance.appearance == null)
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

        // if the client has some changes made in his mecha editor send them back for them to load
        if (player.Data.DIYAppearance != null)
        {
            using var writer = new BinaryUtils.Writer();
            player.Data.DIYAppearance.Export(writer.BinaryWriter);
            player.SendPacket(new PlayerMechaDIYArmor(writer.CloseAndGetBytes(), player.Data.DIYItemId,
                player.Data.DIYItemValue));
        }

        Multiplayer.Session.World.OnAllPlayersSyncCompleted();
    }
}
