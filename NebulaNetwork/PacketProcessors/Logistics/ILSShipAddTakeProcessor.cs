using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class ILSShipAddTakeProcessor : PacketProcessor<ILSShipAddTake>
    {
        public override void ProcessPacket(ILSShipAddTake packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                return;
            }

            Multiplayer.Session.Ships.AddTakeItem(packet);
        }
    }
}
