using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Session;
using NebulaWorld;

namespace NebulaClient.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class JoinSessionConfirmedProcessor : IPacketProcessor<JoinSessionConfirmed>
    {
        public void ProcessPacket(JoinSessionConfirmed packet, NebulaConnection conn)
        {
            LocalPlayer.PlayerId = packet.LocalPlayerId;
        }
    }
}
