using System;
using System.Threading;

namespace NebulaAPI.Simulation;

public delegate void TickEventHandler();

public interface ISimulationTicker
{

    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Invoked every unity Update()
    /// </summary>
    public event TickEventHandler Ticked;

    public void Update();
}
