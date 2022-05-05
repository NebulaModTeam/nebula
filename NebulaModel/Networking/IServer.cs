namespace NebulaModel.Networking
{
    public interface IServer
    {
        int Port { get; }
        string NgrokAddress { get; }
    }
}