using NebulaModel.Attributes;
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
            if (LocalPlayer.GS2_GSSettings != null && packet.CompressedGS2Settings.Length > 1) // if host does not use GS2 we send a null byte
            {
                LocalPlayer.GS2ApplySettings(packet.CompressedGS2Settings);
            }
            else if (LocalPlayer.GS2_GSSettings != null && packet.CompressedGS2Settings.Length == 1)
            {
                InGamePopup.ShowWarning("Galactic Scale - Server not supported", "The server does not seem to use Galactic Scale. Make sure that your mod configuration matches.", "Close");
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
