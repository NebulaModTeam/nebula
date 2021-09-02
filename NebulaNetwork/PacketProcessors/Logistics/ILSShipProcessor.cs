using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class ILSShipProcessor : PacketProcessor<ILSShipData>
    {
        private IPlayerManager playerManager;
        public ILSShipProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(ILSShipData packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                INebulaPlayer player = playerManager.GetPlayer(conn);
                if (player != null)
                {
                    playerManager.SendPacketToOtherPlayers(packet, player);
                }
            }

            // TODO: Shouldn't we call this also on host ??
            if (IsClient)
            {
                using (Multiplayer.Session.Factories.IsIncomingRequest.On())
                {
                    if (packet.idleToWork)
                    {
                        Multiplayer.Session.Ships.IdleShipGetToWork(packet);
                    }
                    else
                    {
                        Multiplayer.Session.Ships.WorkShipBackToIdle(packet);
                    }
                }
            }
        }
    }
}
