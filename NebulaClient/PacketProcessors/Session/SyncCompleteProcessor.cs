using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Session;

namespace NebulaClient.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class SyncCompleteProcessor : IPacketProcessor<SyncComplete>
    {
        public void ProcessPacket(SyncComplete packet, NebulaConnection conn)
        {
            GameMain.Resume();
            // TODO: HIDE PREVIOUSLY OPENED POPUP
        }
    }
}
