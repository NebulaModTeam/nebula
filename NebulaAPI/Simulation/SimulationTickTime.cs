using UnityEngine;

namespace NebulaAPI.Simulation;

public static class SimulationTickTime
{
    public static float DeltaTime => Time.deltaTime;
    public static long GameTick => GameMain.gameTick;
    public static long GameTickOnce => GameMain.onceGameTick;
    public static double GameTime => GameMain.gameTime;
    public static double GameTimeOnce => GameMain.onceGameTime;
}
