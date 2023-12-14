#region

using System;

#endregion

namespace NebulaAPI;

/// <summary>
///     Represents local player. Allows to send packets.
/// </summary>
public interface ILocalPlayer : IDisposable
{
    bool IsInitialDataReceived { get; }
    bool IsHost { get; }
    bool IsClient { get; }
    bool IsNewPlayer { get; }
    ushort Id { get; }
    IPlayerData Data { get; }
}
