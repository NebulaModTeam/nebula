using System;
using NebulaAPI.Simulation;

namespace NebulaAPI.Tasks;

public interface INebulaTask
{
    public ISimulationTicker SimulationTicker { get; }

    /// <summary>
    /// Returns true if the task has started
    /// </summary>
    /// <returns></returns>
    public bool IsRunning { get; }

    /// <summary>
    /// Returns true if this task has finished execution.
    /// </summary>
    public bool IsCompleted { get; }

    /// <summary>
    /// Returns true if the task has been cancelled before completing.
    /// </summary>
    public bool IsCancelled { get; }

    /// <summary>
    /// Instructs the task to stop.
    /// </summary>
    public void Cancel();
}
