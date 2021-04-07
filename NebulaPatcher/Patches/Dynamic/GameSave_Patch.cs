using HarmonyLib;
using NebulaWorld;
using NebulaHost;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameSave))]
    class GameSave_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("SaveCurrentGame")]
        public static void SaveCurrentGame_Prefix(string saveName)
        {
            if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
            {
                SaveManager.SaveServerData(saveName);
            }
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
