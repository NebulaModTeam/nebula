using NebulaAPI.Networking;

namespace NebulaAPI.Packets;

/// <summary>
///     Describes how to process received packets of type T
/// </summary>
/// <typeparam name="T">Packet class</typeparam>
public abstract class BasePacketProcessor<T>
{
    /// <summary>
    ///     Is code running on Host
    /// </summary>
    protected bool IsHost;

    /// <summary>
    ///     Is code running on Client
    /// </summary>
    protected bool IsClient => !IsHost;

    internal void Initialize(bool isHost)
    {
        IsHost = isHost;
    }

    /// <summary>
    ///     Process packets here
    /// </summary>
    /// <param name="packet">Received packet</param>
    /// <param name="conn">Connection that sent the packet</param>
    public abstract void ProcessPacket(T packet, INebulaConnection conn);
}
