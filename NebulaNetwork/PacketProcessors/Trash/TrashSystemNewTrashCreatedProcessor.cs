using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Trash;
using NebulaWorld;
using NebulaWorld.Trash;

namespace NebulaNetwork.PacketProcessors.Trash
{
    [RegisterPacketProcessor]
    class TrashSystemNewTrashCreatedProcessor : PacketProcessor<TrashSystemNewTrashCreatedPacket>
    {
        private PlayerManager playerManager;

        public TrashSystemNewTrashCreatedProcessor()
        {
            playerManager = MultiplayerHostSession.Instance?.PlayerManager;
        }

        public override void ProcessPacket(TrashSystemNewTrashCreatedPacket packet, NebulaConnection conn)
        {
            bool valid = true;
            if (IsHost)
            {
                Player player = playerManager.GetPlayer(conn);
                if (player != null)
                {
                    playerManager.SendPacketToOtherPlayers(packet, player);
                }
                else
                {
                    valid = false;
                }
            }

            if (valid)
            {
                int myId = SimulatedWorld.Instance.GenerateTrashOnPlayer(packet);

                //Check if myID is same as the ID from the host
                if (myId != packet.TrashId)
                {
                    TrashManager.SwitchTrashWithIds(myId, packet.TrashId);
                }
            }
        }
    }
}