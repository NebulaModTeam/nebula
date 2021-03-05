using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIEscMenu), "OnButton5Click")]
    [HarmonyPatch(typeof(UIEscMenu), "OnButton6Click")]
    class UIEscMenu_Patch
    {
        public static void Postfix()
        {
            LocalPlayer.LeaveGame();
        }
    }
}
