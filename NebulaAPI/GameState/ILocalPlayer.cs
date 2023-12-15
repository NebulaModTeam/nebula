#region

using System;

#endregion

namespace NebulaAPI.GameState;

/// <summary>
///     Represents local player. Allows to send packets.
/// </summary>
public interface ILocalPlayer : IDisposable
{
    bool IsInitialDataReceived { get; set; }
    bool IsHost { get; set; }
    bool IsClient { get; }
    bool IsNewPlayer { get; set; }
    ushort Id { get; }
    IPlayerData Data { get; set; }
}
