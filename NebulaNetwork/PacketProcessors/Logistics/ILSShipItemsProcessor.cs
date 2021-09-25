using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    internal class ILSShipItemsProcessor : PacketProcessor<ILSShipItems>
    {
        private readonly IPlayerManager playerManager;
        public ILSShipItemsProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }
        public override void ProcessPacket(ILSShipItems packet, NebulaConnection conn)
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
                // TODO: Aren't we missing a using (incoming.On()) here ?
                Multiplayer.Session.Ships.AddTakeItem(packet);
            }
        }
    }
}
