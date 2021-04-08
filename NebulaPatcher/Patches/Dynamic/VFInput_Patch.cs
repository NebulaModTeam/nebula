using HarmonyLib;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(VFInput))]
    class VFInput_Patch
    {
        [HarmonyPatch("_buildConfirm", MethodType.Getter)]
        static bool Prefix(ref VFInput.InputValue __result)
        {
            if (FactoryManager.EventFromServer)
            {
                __result = default(VFInput.InputValue);
                __result.onDown = true;
                return false;
            }
            return true;
        }
    }
}
