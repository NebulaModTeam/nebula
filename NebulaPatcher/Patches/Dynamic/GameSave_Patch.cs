using HarmonyLib;
using NebulaNetwork;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameSave))]
    class GameSave_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameSave.SaveCurrentGame))]
        public static bool SaveCurrentGame_Prefix(string saveName)
        {
            if (Multiplayer.IsActive && ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost)
            {
                SaveManager.SaveServerData(saveName);
            }

            // Only save if in single player or if you are the host
            return (!Multiplayer.IsActive && !Multiplayer.IsLeavingGame) || ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameSave.AutoSave))]
        public static bool AutoSave_Prefix()
        {
            // Only save if in single player or if you are the host
            return !Multiplayer.IsActive || ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameSave.SaveAsLastExit))]
        public static bool SaveAsLastExit_Prefix()
        {
            // Only save if in single player, since multiplayer requires to load from the Load Save Window
            return (!Multiplayer.IsActive && !Multiplayer.IsLeavingGame);
        }
    }
}
