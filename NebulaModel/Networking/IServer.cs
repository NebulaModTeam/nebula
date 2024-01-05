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
    /// Send a packet to all players that match a predicate
    /// </summary>
    /// <param name="packet"></param>
    /// <param name="condition"></param>
    /// <typeparam name="T"></typeparam>
    public void SendIfCondition<T>(T packet, Predicate<INebulaPlayer> condition) where T : class, new();
}
