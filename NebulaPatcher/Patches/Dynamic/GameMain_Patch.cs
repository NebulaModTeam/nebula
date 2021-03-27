using HarmonyLib;
using NebulaWorld;
namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameMain), "Pause")]
    class GameMain_Patch
    {
        public static bool Prefix()
        {
            //Do not pause game in the multiplayer
            //Pausing game has to be done via: GameMain.instance._paused = true;
            return !SimulatedWorld.Initialized;
        }
    }
}
