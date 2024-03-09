#region

using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Combat;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIDFCommunicatorWindow))]
internal class UIDFCommunicatorWindow_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIDFCommunicatorWindow._OnOpen))]
    public static bool UIDFCommunicatorWindow_OnOpen_Prefix(UIDFCommunicatorWindow __instance)
    {
        if (!Multiplayer.IsActive) return true;

        if (EnemyManager.DISABLE_DFCommunicator)
        {
            InGamePopup.ShowInfo("Unavailable".Translate(), "Dark Fog Communicator is disabled in multiplayer game.".Translate(),
                "OK".Translate());
            __instance._Close();
        }
        return false;
    }
}
