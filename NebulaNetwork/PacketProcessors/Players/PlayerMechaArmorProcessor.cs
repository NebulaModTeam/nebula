using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;
using System.Collections.Generic;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerMechaArmorProcessor: PacketProcessor<PlayerMechaArmor>
    {
        private readonly IPlayerManager playerManager;

        public PlayerMechaArmorProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(PlayerMechaArmor packet, NebulaConnection conn)
        {
            INebulaPlayer player = null;
            if (IsHost)
            {
                // broadcast to other players
                player = playerManager.GetPlayer(conn);
                if (player != null)
                {
                    playerManager.SendPacketToOtherPlayers(packet, player);
                }
            }

            using (BinaryUtils.Reader reader = new BinaryUtils.Reader(packet.AppearanceData))
            {
                if(Multiplayer.Session.LocalPlayer.Id == packet.PlayerId)
                {
                    GameMain.mainPlayer.mecha.appearance.Import(reader.BinaryReader);
                    GameMain.mainPlayer.mechaArmorModel.RefreshAllPartObjects();
                    GameMain.mainPlayer.mechaArmorModel.RefreshAllBoneObjects();
                    GameMain.mainPlayer.mecha.appearance.NotifyAllEvents();
                    GameMain.mainPlayer.mechaArmorModel._Init(GameMain.mainPlayer);
                    GameMain.mainPlayer.mechaArmorModel._OnOpen();
                }
                else
                {
                    using (Multiplayer.Session.World.GetRemotePlayersModels(out Dictionary<ushort, RemotePlayerModel> remotePlayersModels))
                    {
                        if (remotePlayersModels.TryGetValue(packet.PlayerId, out RemotePlayerModel playerModel))
                        {
                            playerModel.MechaInstance.appearance.Import(reader.BinaryReader);
                            playerModel.PlayerInstance.mechaArmorModel.RefreshAllPartObjects();
                            playerModel.PlayerInstance.mechaArmorModel.RefreshAllBoneObjects();
                            playerModel.MechaInstance.appearance.NotifyAllEvents();
                            playerModel.PlayerInstance.mechaArmorModel._Init(playerModel.PlayerInstance);
                            playerModel.PlayerInstance.mechaArmorModel._OnOpen();

                            // and store the appearance on the server
                            if (IsHost && player != null)
                            {
                                if (player.Data.Appearance == null)
                                {
                                    player.Data.Appearance = new MechaAppearance();
                                    player.Data.Appearance.Init();
                                }
                                playerModel.MechaInstance.appearance.CopyTo(player.Data.Appearance);
                            }
                        }
                    }
                }
            }
        }
    }
}
