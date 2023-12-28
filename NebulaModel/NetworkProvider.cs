#region

using System;
using System.Threading;
using NebulaAPI.GameState;
using NebulaAPI.Packets;
using NebulaAPI.Simulation;
using NebulaAPI.Tasks;
using NebulaModel.Networking.Serialization;
using UnityEngine;

#endregion

namespace NebulaModel;

public abstract class NetworkProvider : INetworkProvider
{
    protected NetworkProvider(IPlayerManager playerManager)
    {
        PacketProcessor = new NebulaNetPacketProcessor();
        FrameTicker = new SimulationTicker(CancellationTokenSource);
        SimulationTicker = new SimulationTicker(CancellationTokenSource);
        PlayerManager = playerManager;
    }

    protected readonly CancellationTokenSource CancellationTokenSource = new();

    public NebulaNetPacketProcessor PacketProcessor { get; set; }

    public ISimulationTicker FrameTicker { get; }

    public ISimulationTicker SimulationTicker { get; }

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

    public virtual void Update()
    {
        FrameTicker.Update();
    }

    public virtual void SimulationUpdate()
    {
        // SimulationTicker.Update(deltaTime);
    }

    public abstract void Start();

    public abstract void Stop();

    public virtual void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}
