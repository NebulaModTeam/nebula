using NebulaAPI;
using NebulaModel.Networking;

namespace NebulaModel.Packets
{
    public abstract class PacketProcessor<T> : BasePacketProcessor<T>
    {
        public override void ProcessPacket(T packet, INebulaConnection conn)
        {
            ProcessPacket(packet, (NebulaConnection)conn);
        }

        public abstract void ProcessPacket(T packet, NebulaConnection conn);
    }
}
