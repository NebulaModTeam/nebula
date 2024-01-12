#region

using System;
using BepInEx.Bootstrap;
using NebulaAPI.Interfaces;
using NebulaAPI.Packets;
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
    protected override void ProcessPacket(LobbyResponse packet, NebulaConnection conn)
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
        gameDesc.isPeaceMode = packet.IsPeaceMode;
        gameDesc.isSandboxMode = packet.IsSandboxMode;
        gameDesc.savedThemeIds = new int[packet.SavedThemeIds.Length];
        Array.Copy(packet.SavedThemeIds, gameDesc.savedThemeIds, packet.SavedThemeIds.Length);
        using (var p = new BinaryUtils.Reader(packet.CombatSettingsData))
        {
            gameDesc.combatSettings.Import(p.BinaryReader);
        }

        UIRoot.instance.galaxySelect.gameDesc = gameDesc;
        UIRoot.instance.galaxySelect.SetStarmapGalaxy();
        UIRoot.instance.galaxySelect.sandboxToggle.isOn = gameDesc.isSandboxMode;
    }
}
