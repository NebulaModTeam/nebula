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
            if (SaveManager.SaveOnExit || SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
            {
                SaveManager.SaveServerData(saveName);
                SaveManager.SaveOnExit = false;
            }

            if (SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient)
            {
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("LoadCurrentGame")]
        public static void LoadCurrentGame_Prefix(string saveName)
        {
            SaveManager.SetLastSave(saveName);
        }

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
