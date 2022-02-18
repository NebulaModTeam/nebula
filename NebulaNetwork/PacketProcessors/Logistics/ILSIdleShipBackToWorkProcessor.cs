using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class ILSIdleShipBackToWorkProcessor : PacketProcessor<ILSIdleShipBackToWork>
    {
        public override void ProcessPacket(ILSIdleShipBackToWork packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                return;
            }

            if (IsClient)
            {
                using (Multiplayer.Session.Factories.IsIncomingRequest.On())
                {
                    Multiplayer.Session.Ships.IdleShipGetToWork(packet);
                }
            }
        }
    }
}
