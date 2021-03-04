using NebulaModel.Networking;

namespace NebulaModel.Packets.Processors
{
    public interface IPacketProcessor<T>
    {
        void ProcessPacket(T packet, NebulaConnection conn);
    }
}
