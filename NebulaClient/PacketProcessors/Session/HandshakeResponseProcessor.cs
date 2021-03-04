using NebulaClient.GameLogic;
using NebulaClient.MonoBehaviours;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Session;

namespace NebulaClient.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class HandshakeResponseProcessor : IPacketProcessor<HandshakeResponse>
    {
        private PlayerManager playerManager;

        public HandshakeResponseProcessor()
        {
            playerManager = MultiplayerClientSession.Instance.PlayerManager;
        }

        public void ProcessPacket(HandshakeResponse packet, NebulaConnection conn)
        {
            // playerManager.SetLocalPlayer(packet.LocalPlayerID);

            // TODO: Make our own InGameMessageBox class that will keep the reference of the currently open popup internally instead.
            // UIMessageBox.Show("Loading", "Loading state from server, please wait", null, UIMessageBox.INFO);

            foreach (var playerId in packet.OtherPlayerIds)
            {
                playerManager.AddRemotePlayer(playerId);
            }
        }
    }
}
