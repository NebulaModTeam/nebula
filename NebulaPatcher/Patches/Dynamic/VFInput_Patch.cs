using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(VFInput))]
    class VFInput_Patch
    {
        [HarmonyPatch(nameof(VFInput._buildConfirm), MethodType.Getter)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static bool _buildConfirm_Prefix(ref VFInput.InputValue __result)
        {
            if (Multiplayer.IsActive && Multiplayer.Session.Factories.IsIncomingRequest.Value)
            {
                __result = default;
                __result.onDown = true;
                return false;
            }
            return true;
        }
    }
}
