namespace NebulaModel.Networking
{
    public interface IServer
    {
        ushort Port { get; }
        string NgrokAddress { get; }
        bool NgrokActive { get; }
    }
}