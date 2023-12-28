using NebulaAPI.Tasks;

namespace NebulaAPI.Simulation;

public interface ISimulation
{
    ISimulationTicker FrameTicker { get; }

    ISimulationTicker SimulationTicker { get; }

    /// <summary>
    /// Unity Update(), happens every frame.
    /// </summary>
    public void Update();

    /// <summary>
    /// Happens after GameData.GameTick().
    /// </summary>
    public void SimulationUpdate();
}
