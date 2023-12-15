namespace NebulaModel.Networking;

public interface IServer
{
    ushort Port { get; set; }
    string NgrokAddress { get; }
    bool NgrokActive { get; }
    bool NgrokEnabled { get; }
    string NgrokLastErrorCode { get; }
}
