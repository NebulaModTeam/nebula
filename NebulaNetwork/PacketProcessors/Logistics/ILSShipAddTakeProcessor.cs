using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    internal class ILSShipAddTakeProcessor : PacketProcessor<ILSShipAddTake>
    {
        private readonly IPlayerManager playerManager;
        public ILSShipAddTakeProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }
        public override void ProcessPacket(ILSShipAddTake packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                return;
            }

            if (IsClient)
            {
                Multiplayer.Session.Ships.AddTakeItem(packet);
            }
        }
    }
}
