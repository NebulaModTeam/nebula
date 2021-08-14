namespace NebulaAPI
{
    public abstract class BasePacketProcessor<T>
    {
        protected bool IsHost;
        protected bool IsClient => !IsHost;

        internal void Initialize(bool isHost)
        {
            IsHost = isHost;
        }

        public abstract void ProcessPacket(T packet, INebulaConnection conn);
    }
}