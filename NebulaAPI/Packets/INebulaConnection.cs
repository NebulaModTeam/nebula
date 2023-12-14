#region

using System;

#endregion

namespace NebulaAPI.Packets;

// Use Equals() to check value equality
public interface INebulaConnection : IEquatable<INebulaConnection>
{
    void SendPacket<T>(T packet) where T : class, new();

    void SendRawPacket(byte[] rawData);
}
