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

        public override void ProcessPacket(StationUI packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                // always update values for host
                packet.ShouldRefund = false;
                Multiplayer.Session.StationsUI.UpdateStation(ref packet);

                // broadcast to other clients 
                IPlayerManager playerManager = Multiplayer.Session.Network.PlayerManager;
                INebulaPlayer player = playerManager.GetPlayer(conn);
                playerManager.SendPacketToOtherPlayers(packet, player);

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
