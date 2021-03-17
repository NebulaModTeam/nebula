namespace NebulaWorld
{
    public interface INetworkProvider
    {
        void SendPacket<T>(T packet) where T : class, new();

        void DestroySession();
    }
}
