#region

using BepInEx.Bootstrap;
using NebulaAPI.Interfaces;
using NebulaAPI.Packets;
using NebulaModel;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;
using NebulaWorld;
using NebulaWorld.SocialIntegration;

#endregion

namespace NebulaNetwork.PacketProcessors.Session;

[RegisterPacketProcessor]
public class HandshakeResponseProcessor : PacketProcessor<HandshakeResponse>
{
    protected override void ProcessPacket(HandshakeResponse packet, NebulaConnection conn)
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

        // overwrite local setting with host setting, but dont save it as its a temp setting for this session
        Config.Options.SyncSoil = packet.SyncSoil;

        ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost = false;
        ((LocalPlayer)Multiplayer.Session.LocalPlayer).SetPlayerData(packet.LocalPlayerData, packet.IsNewPlayer);

        Multiplayer.Session.IsInLobby = false;
        Multiplayer.ShouldReturnToJoinMenu = false;

        // Using GameDesc.Import will break GS2, so use GameDesc.SetForNewGame instead
        var gameDesc = new GameDesc();
        gameDesc.SetForNewGame(packet.GalaxyAlgo, packet.GalaxySeed, packet.StarCount, 1, packet.ResourceMultiplier);
        gameDesc.isPeaceMode = packet.IsPeaceMode;
        gameDesc.isSandboxMode = packet.IsSandboxMode;
        gameDesc.savedThemeIds = packet.SavedThemeIds;
        using (var p = new BinaryUtils.Reader(packet.CombatSettingsData))
        {
            gameDesc.combatSettings.Import(p.BinaryReader);
        }
        DSPGame.StartGameSkipPrologue(gameDesc);

        InGamePopup.ShowInfo("Loading".Translate(), "Loading state from server, please wait".Translate(), null);

        Multiplayer.Session.NumPlayers = packet.NumPlayers;
        DiscordManager.UpdateRichPresence(partyId: packet.DiscordPartyId);
    }
}
