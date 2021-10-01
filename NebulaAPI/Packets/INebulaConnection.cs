namespace NebulaAPI
{
    public interface INebulaConnection
    {
        void SendPacket<T>(T packet) where T : class, new();
    }
}