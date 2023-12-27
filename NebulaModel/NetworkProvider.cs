#region

using NebulaAPI.GameState;
using NebulaAPI.Packets;
using NebulaModel.Networking.Serialization;

#endregion

namespace NebulaModel;

public abstract class NetworkProvider : INetworkProvider
{
    protected NetworkProvider(IPlayerManager playerManager)
    {
        PacketProcessor = new NebulaNetPacketProcessor();
        PlayerManager = playerManager;
    }

    public NebulaNetPacketProcessor PacketProcessor { get; set; }

    public abstract void Dispose();

    public IPlayerManager PlayerManager { get; set; }

    public abstract void SendPacket<T>(T packet) where T : class, new();

    public abstract void SendPacketToLocalStar<T>(T packet) where T : class, new();

    public abstract void SendPacketToLocalPlanet<T>(T packet) where T : class, new();

    public abstract void SendPacketToPlanet<T>(T packet, int planetId) where T : class, new();

    public abstract void SendPacketToStar<T>(T packet, int starId) where T : class, new();

    public abstract void SendPacketExclude<T>(T packet, INebulaConnection exclude)
        where T : class, new();

    public abstract void SendPacketToStarExclude<T>(T packet, int starId, INebulaConnection exclude)
        where T : class, new();

    public abstract void Update();

    public abstract void Start();

    public abstract void Stop();
}
