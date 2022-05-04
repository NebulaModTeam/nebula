using BepInEx;
using BepInEx.Bootstrap;
using NebulaAPI;
using NebulaModel;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;
using NebulaWorld;
using NebulaWorld.SocialIntegration;

namespace NebulaNetwork.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class HandshakeResponseProcessor : PacketProcessor<HandshakeResponse>
    {
        public override void ProcessPacket(HandshakeResponse packet, NebulaConnection conn)
        {
            using (BinaryUtils.Reader p = new BinaryUtils.Reader(packet.ModsSettings))
            {
                for (int i = 0; i < packet.ModsSettingsCount; i++)
                {
                    string guid = p.BinaryReader.ReadString();
                    PluginInfo info = Chainloader.PluginInfos[guid];
                    if (info.Instance is IMultiplayerModWithSettings mod)
                    {
                        mod.Import(p.BinaryReader);
                    }
                }
            }

            // overwrite local setting with host setting, but dont save it as its a temp setting for this session
            Config.Options.SyncSoil = packet.SyncSoil;

            ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost = false;
            ((LocalPlayer)Multiplayer.Session.LocalPlayer).SetPlayerData(packet.LocalPlayerData, packet.IsNewPlayer);

            Multiplayer.Session.IsInLobby = false;
            Multiplayer.ShouldReturnToJoinMenu = false;

            // Using GameDesc.Import will break GS2, so use GameDesc.SetForNewGame instead
            GameDesc gameDesc = new GameDesc();
            gameDesc.SetForNewGame(packet.AlgoVersion, packet.GalaxySeed, packet.StarCount, 1, packet.ResourceMultiplier);
            gameDesc.savedThemeIds = packet.SavedThemeIds;
            DSPGame.StartGameSkipPrologue(gameDesc);

            InGamePopup.ShowInfo("Loading", "Loading state from server, please wait", null);

            Multiplayer.Session.NumPlayers = packet.NumPlayers;
            DiscordManager.UpdateRichPresence(partyId: packet.DiscordPartyId);
        }
    }
}
