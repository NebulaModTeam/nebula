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
            GameDesc gameDesc = new GameDesc();
            gameDesc.SetForNewGame(packet.AlgoVersion, packet.GalaxySeed, packet.StarCount, 1, packet.ResourceMultiplier);
            DSPGame.StartGameSkipPrologue(gameDesc);

            Multiplayer.Session.LocalPlayer.IsHost = false;
            Multiplayer.Session.LocalPlayer.SetPlayerData(packet.LocalPlayerData);

            InGamePopup.ShowInfo("Loading", "Loading state from server, please wait", null);
        }
    }
}
