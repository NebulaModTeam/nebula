using HarmonyLib;
using NebulaPatcher.Patches.Transpilers;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(FactorySystem))]
    class FactorySystem_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(FactorySystem.RemoveInserterComponent))]
        public static bool RemoveInserterComponent_Prefix(FactorySystem __instance, int id)
        {
            if (!SimulatedWorld.Initialized || !LocalPlayer.IsMasterClient)
            {
                return true;
            }

            foreach (var obj in InserterComponent_Transpiler.FaultySortersText)
            {
                if (obj.Value?.transform.position == __instance.inserterPool[id].pos2)
                {
                    obj.Value.SetActive(false);
                    break;
                }
            }

            return true;
        }
    }
}
