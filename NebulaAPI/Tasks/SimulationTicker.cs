using System;
using System.Threading;
using NebulaAPI.Simulation;

namespace NebulaAPI.Tasks;

public class SimulationTicker(CancellationTokenSource cts) : ISimulationTicker
{
    /// <summary>
    /// A CTS to use in case we weren't provided a cancellation token explicitly.
    /// </summary>
    private CancellationTokenSource CancellationTokenSource { get; } = cts;

    public CancellationToken CancellationToken => CancellationTokenSource.Token;

    public event TickEventHandler Ticked;

    ~SimulationTicker()
    {
        RequestCancellation();
    }

    public void Update()
    {
        Ticked?.Invoke();
    }

    /// <summary>
    /// Request the cancellation of all pending tasks bound to this ticker.
    /// </summary>
    public void RequestCancellation() => CancellationTokenSource.Cancel();
}
