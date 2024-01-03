using System;
using NebulaAPI.GameState;
using NebulaAPI.Networking;
using NebulaAPI.Packets;

namespace NebulaModel.Networking;

public interface IServer : INetworkProvider
{
    ushort Port { get; set; }
    string NgrokAddress { get; }
    bool NgrokActive { get; }
    bool NgrokEnabled { get; }
    string NgrokLastErrorCode { get; }
    public event EventHandler<INebulaConnection> Connected;
    public event EventHandler<INebulaConnection> Disconnected;

    public void Update();

    public void Start();

    public void Stop();
}
