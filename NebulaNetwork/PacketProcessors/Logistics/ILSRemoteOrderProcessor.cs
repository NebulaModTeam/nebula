using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    internal class ILSRemoteOrderProcessor : PacketProcessor<ILSRemoteOrderData>
    {
        private readonly IPlayerManager playerManager;

        public ILSRemoteOrderProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(ILSRemoteOrderData packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                INebulaPlayer player = playerManager.GetPlayer(conn);
                if (player != null)
                {
                    playerManager.SendPacketToOtherPlayers(packet, player);
                }

                // TODO: Don't we need to call Multiplayer.Session.World.OnILSRemoteOrderUpdate() here too ??
            }

            if (IsClient)
            {
                // TODO: Don't we need a using(incomingPacket.On())
                Multiplayer.Session.Ships.UpdateRemoteOrder(packet);
            }
        }
    }
}
