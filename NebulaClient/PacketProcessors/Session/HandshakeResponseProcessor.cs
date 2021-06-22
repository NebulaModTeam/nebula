using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Session;
using NebulaWorld;

namespace NebulaClient.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class HandshakeResponseProcessor : IPacketProcessor<HandshakeResponse>
    {
        public void ProcessPacket(HandshakeResponse packet, NebulaConnection conn)
        {
            if(LocalPlayer.GS2_GSSettings != null && packet.CompressedGS2Settings != null)
            {
                LocalPlayer.GS2ApplySettings(packet.CompressedGS2Settings);
            }
            else if(packet.CompressedGS2Settings == null)
            {
                InGamePopup.ShowWarning("Galactic Scale - failed to receive settings", "We are sorry, but for some reason the server failed to export the Galactic Scale 2 settings.", "Close");
                return;
            }

            GameDesc gameDesc = new GameDesc();
            gameDesc.SetForNewGame(packet.AlgoVersion, packet.GalaxySeed, packet.StarCount, 1, packet.ResourceMultiplier);
            DSPGame.StartGameSkipPrologue(gameDesc);

            LocalPlayer.IsMasterClient = false;
            LocalPlayer.SetPlayerData(packet.LocalPlayerData);

            InGamePopup.ShowInfo("Loading", "Loading state from server, please wait", null);
        }
    }
}
