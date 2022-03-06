using NebulaModel.Logger;
using System;

namespace NebulaWorld.GameStates
{
    public class GameStatesManager : IDisposable
    {

        public GameStatesManager()
        {
        }

        public void Dispose()
        {
        }

        public static long RealGameTick => GameMain.gameTick;
        public static float RealUPS => (float)FPSController.currentUPS;

        public static void NotifyTickDifference(float delta)
        {
        }
    }
}
