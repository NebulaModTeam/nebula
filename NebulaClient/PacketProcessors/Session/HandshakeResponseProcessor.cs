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
            GameDesc gameDesc = new GameDesc();
            gameDesc.SetForNewGame(packet.AlgoVersion, packet.GalaxySeed, packet.StarCount, 1, packet.ResourceMultiplier);
            DSPGame.StartGameSkipPrologue(gameDesc);

            LocalPlayer.IsMasterClient = false;
            LocalPlayer.SetPlayerData(packet.LocalPlayerData);

            InGamePopup.ShowInfo("Loading", "Loading state from server, please wait", null);
        }
    }
}
