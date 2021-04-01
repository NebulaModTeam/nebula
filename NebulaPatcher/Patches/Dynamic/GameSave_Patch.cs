using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameSave))]
    class GameSave_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("AutoSave")]
        public static bool AutoSave_Prefix()
        {
            //Do not trigger autosave for the clients in multiplayer
            return !SimulatedWorld.Initialized || LocalPlayer.IsMasterClient;
        }

        [HarmonyPrefix]
        [HarmonyPatch("SaveAsLastExit")]
        public static bool SaveAsLastExit_Prefix()
        {
            //Do not trigger autosave for the clients in multiplayer
            return !SimulatedWorld.Initialized || LocalPlayer.IsMasterClient;
        }
    }
}
