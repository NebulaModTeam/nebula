using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    internal class StationUIProcessor : PacketProcessor<StationUI>
    {
        private readonly IPlayerManager playerManager;
        public StationUIProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(StationUI packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                // always update values for host
                packet.ShouldRefund = false;
                Multiplayer.Session.StationsUI.UpdateStation(ref packet);

                // broadcast to every clients that may have the station loaded.
                int starId = GameMain.galaxy.PlanetById(packet.PlanetId)?.star.id ?? -1;
                playerManager.SendPacketToStarExcept(packet, starId, conn);

                // as we block the normal method for the client he must run it once he receives this packet.
                // but only the one issued the request should get items refund
                packet.ShouldRefund = true;
                conn.SendPacket(packet);
            }

            if (IsClient)
            {
                Multiplayer.Session.StationsUI.UpdateStation(ref packet);
            }
        }
    }
}
