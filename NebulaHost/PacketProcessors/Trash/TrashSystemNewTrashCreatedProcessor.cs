using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Trash;
using NebulaWorld;

namespace NebulaHost.PacketProcessors.Statistics
{
    [RegisterPacketProcessor]
    class TrashSystemNewTrashCreatedProcessor : IPacketProcessor<TrashSystemNewTrashCreatedPacket>
    {
        private PlayerManager playerManager;

        public TrashSystemNewTrashCreatedProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(TrashSystemNewTrashCreatedPacket packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                int myId = SimulatedWorld.GenerateTrashOnPlayer(packet);
               
                //Send to other players trash with valid ID
                playerManager.SendPacketToOtherPlayers(packet, player);

                //Send correction packet to the creator with authotaritive ID if the ID does not match
                if (myId != packet.TrashId) {
                    player.SendPacket(new TrashSystemCorrectionIdPacket(packet.TrashId, myId));
                }
            }
        }
    }
}