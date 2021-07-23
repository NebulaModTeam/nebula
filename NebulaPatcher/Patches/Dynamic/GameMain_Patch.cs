using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameMain))]
    class GameMain_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameMain.Pause))]
        public static bool Pause_Prefix()
        {
            //Do not pause game in the multiplayer
            //Pausing game has to be done via: GameMain.instance._paused = true;
            return !SimulatedWorld.Initialized;
        }
    }
}
