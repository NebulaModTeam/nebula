using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    class CreatePrebuildsRequestProcessor : PacketProcessor<CreatePrebuildsRequest>
    {
        public override void ProcessPacket(CreatePrebuildsRequest packet, NebulaConnection conn)
        {
            using (Multiplayer.Session.Factories.IsIncomingRequest.On())
            {
                Multiplayer.Session.BuildTools.CreatePrebuildsRequest(packet);
            }
        }
    }
}
