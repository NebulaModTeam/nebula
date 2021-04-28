using HarmonyLib;
using NebulaHost;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameSave))]
    class GameSave_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("SaveCurrentGame")]
        public static bool SaveCurrentGame_Prefix(string saveName)
        {
            if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
            {
                SaveManager.SaveServerData(saveName);
            }

            // Only save if in single player or if you are the host
            return (!SimulatedWorld.Initialized && !SimulatedWorld.ExitingMultiplayerSession) || LocalPlayer.IsMasterClient;
        }

        [HarmonyPrefix]
        [HarmonyPatch("AutoSave")]
        public static bool AutoSave_Prefix()
        {
            // Only save if in single player or if you are the host
            return !SimulatedWorld.Initialized || LocalPlayer.IsMasterClient;
        }

        [HarmonyPrefix]
        [HarmonyPatch("SaveAsLastExit")]
        public static bool SaveAsLastExit_Prefix()
        {
            // Only save if in single player, since multiplayer requires to load from the Load Save Window
            return (!SimulatedWorld.Initialized && !SimulatedWorld.ExitingMultiplayerSession);
        }
    }
}
