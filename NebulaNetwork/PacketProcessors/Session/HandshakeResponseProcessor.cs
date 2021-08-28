using BepInEx;
using BepInEx.Bootstrap;
using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;
using NebulaWorld;

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
            ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost = false;
            ((LocalPlayer)Multiplayer.Session.LocalPlayer).SetPlayerData(packet.LocalPlayerData, packet.IsNewPlayer);

            GameDesc gameDesc = new GameDesc();
            gameDesc.SetForNewGame(packet.AlgoVersion, packet.GalaxySeed, packet.StarCount, 1, packet.ResourceMultiplier);
            DSPGame.StartGameSkipPrologue(gameDesc);

            InGamePopup.ShowInfo("Loading", "Loading state from server, please wait", null);
        }
    }
}
