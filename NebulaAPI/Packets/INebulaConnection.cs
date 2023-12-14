#region

using System;

#endregion

namespace NebulaAPI;

// Use Equals() to check value equality
public interface INebulaConnection : IEquatable<INebulaConnection>
{
    void SendPacket<T>(T packet) where T : class, new();

    void SendRawPacket(byte[] rawData);
}
