#region

using System;

#endregion

namespace NebulaAPI.Networking;

public enum EConnectionStatus
{
    None = 0,
    Pending,
    Syncing,
    Connected
}

// Use Equals() to check value equality
public interface INebulaConnection : IEquatable<INebulaConnection>
{
    public bool IsAlive { get; }

    public int Id { get; }

    public EConnectionStatus ConnectionStatus { get; set; }

    void SendPacket<T>(T packet) where T : class, new();

    void SendRawPacket(byte[] rawData);
}
