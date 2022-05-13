using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIAbnormalityTip))]
    internal class UIAbnormalityTip_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIAbnormalityTip._OnOpen))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnOpen_Prefix(UIAbnormalityTip __instance)
        {
            // Suppress runtime errror reports on client because client can't unlock achievements anyway in host's save
            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsClient)
            {
                __instance._Close();
            }
        }
    }
}
