using System;
using System.Collections.Generic;
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

    public IReadOnlyDictionary<INebulaConnection, INebulaPlayer> PlayerConnections { get; }
    public IReadOnlyCollection<INebulaPlayer> Players { get; }

    public void Update();

    public void Start();

    public void Stop();

    void Disconnect(INebulaConnection conn, DisconnectionReason reason, string reasonMessage = "");

    /// <summary>
    /// Send to a specified collection of players.
    /// </summary>
    /// <param name="players"></param>
    /// <param name="packet"></param>
    /// <typeparam name="T"></typeparam>
    public void SendTo<T>(IEnumerable<INebulaPlayer> players, T packet) where T : class, new();
}
