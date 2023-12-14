#region

using BepInEx.Bootstrap;
using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;
using NebulaWorld;
using NebulaWorld.SocialIntegration;

#endregion

namespace NebulaNetwork.PacketProcessors.Session;

[RegisterPacketProcessor]
internal class LobbyResponseProcessor : PacketProcessor<LobbyResponse>
{
    public override void ProcessPacket(LobbyResponse packet, NebulaConnection conn)
    {
        using (var p = new BinaryUtils.Reader(packet.ModsSettings))
        {
            for (var i = 0; i < packet.ModsSettingsCount; i++)
            {
                var guid = p.BinaryReader.ReadString();
                var info = Chainloader.PluginInfos[guid];
                if (info.Instance is IMultiplayerModWithSettings mod)
                {
                    mod.Import(p.BinaryReader);
                }
            }
        }
        ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost = false;
        Multiplayer.Session.NumPlayers = packet.NumPlayers;
        Multiplayer.Session.IsInLobby = true;
        DiscordManager.UpdateRichPresence(partyId: packet.DiscordPartyId);

        UIRoot.instance.galaxySelect._Open();
        UIRoot.instance.uiMainMenu._Close();

        var gameDesc = new GameDesc();
        gameDesc.SetForNewGame(packet.GalaxyAlgo, packet.GalaxySeed, packet.StarCount, 1, packet.ResourceMultiplier);
        gameDesc.savedThemeIds = packet.SavedThemeIds;
        gameDesc.isSandboxMode = packet.IsSandboxMode;

        UIRoot.instance.galaxySelect.gameDesc = gameDesc;
        UIRoot.instance.galaxySelect.SetStarmapGalaxy();
        UIRoot.instance.galaxySelect.sandboxToggle.isOn = packet.IsSandboxMode;
    }
}
