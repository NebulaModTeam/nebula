using Mirror;

namespace NebulaModel.Packets
{
    public abstract class PacketProcessor<T>
    {
        protected bool IsHost;
        protected bool IsClient => !IsHost;

        internal void Initialize(bool isHost)
        {
            IsHost = isHost;
        }

        public abstract void ProcessPacket(T packet, NetworkConnection conn);
    }
}
