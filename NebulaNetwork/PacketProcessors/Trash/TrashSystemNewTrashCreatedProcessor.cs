using NebulaAPI;
using NebulaModel;
using NebulaModel.Attributes;
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
        private IPlayerManager playerManager;

        public TrashSystemNewTrashCreatedProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(TrashSystemNewTrashCreatedPacket packet, NebulaConnection conn)
        {
            bool valid = true;
            if (IsHost)
            {
                NebulaPlayer player = playerManager.GetPlayer(conn);
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
                int myId = Multiplayer.Session.World.GenerateTrashOnPlayer(packet);

                //Check if myID is same as the ID from the host
                if (myId != packet.TrashId)
                {
                    Multiplayer.Session.Trashes.SwitchTrashWithIds(myId, packet.TrashId);
                }
            }
        }
    }
}