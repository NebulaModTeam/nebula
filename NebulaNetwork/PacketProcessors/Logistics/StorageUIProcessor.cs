using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    internal class StorageUIProcessor : PacketProcessor<StorageUI>
    {
        private readonly IPlayerManager playerManager;
        public StorageUIProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(StorageUI packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                // always update values for host
                packet.ShouldRefund = false;
                Multiplayer.Session.StationsUI.UpdateStorage(packet);

                // broadcast to every clients that may have the station loaded.
                int starId = GameMain.galaxy.PlanetById(packet.PlanetId)?.star.id ?? -1;
                playerManager.SendPacketToStarExcept(packet, starId, conn);

                // as we block some methods for the client he must run it once he receives this packet.
                // but only the one issued the request should get items refund
                packet.ShouldRefund = true;
                conn.SendPacket(packet);
            }

            if (IsClient)
            {
                Multiplayer.Session.StationsUI.UpdateStorage(packet);
            }
        }
    }
}
