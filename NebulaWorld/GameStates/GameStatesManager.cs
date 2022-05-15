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
        public static bool DuringReconnect = false;
        public static string ImportedSaveName { get; set; }

        public static void NotifyTickDifference(float delta)
        {
        }

        public static void DoFastReconnect()
        {
            // trigger game exit to main menu
            DuringReconnect = true;
            UIRoot.instance.uiGame.escMenu.OnButton5Click();
        }
    }
}
