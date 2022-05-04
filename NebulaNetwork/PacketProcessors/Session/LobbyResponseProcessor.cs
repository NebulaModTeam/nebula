using BepInEx;
using BepInEx.Bootstrap;
using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;
using NebulaWorld;
using NebulaWorld.SocialIntegration;

namespace NebulaNetwork.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    class LobbyResponseProcessor: PacketProcessor<LobbyResponse>
    {
        public override void ProcessPacket(LobbyResponse packet, NebulaConnection conn)
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
            ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost = false;
            Multiplayer.Session.NumPlayers = packet.NumPlayers;
            Multiplayer.Session.IsInLobby = true;
            DiscordManager.UpdateRichPresence();

            UIRoot.instance.galaxySelect._Open();
            UIRoot.instance.uiMainMenu._Close();

            GameDesc gameDesc = new GameDesc();
            gameDesc.SetForNewGame(packet.GalaxyAlgo, packet.GalaxySeed, packet.StarCount, 1, packet.ResourceMultiplier);
            gameDesc.savedThemeIds = packet.SavedThemeIds;

            UIRoot.instance.galaxySelect.gameDesc = gameDesc;
            UIRoot.instance.galaxySelect.SetStarmapGalaxy();
        }
    }
}
