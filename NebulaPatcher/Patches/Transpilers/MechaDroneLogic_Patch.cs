using HarmonyLib;
using NebulaWorld.Player;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpiler
{
    [HarmonyPatch(typeof(MechaDroneLogic))]
    class MechaDroneLogic_Patch
    {
        /*
         * Call DroneManager.BroadcastDroneOrder(int droneId, int entityId) when drone gets new order
         */
        [HarmonyTranspiler]
        [HarmonyPatch("UpdateTargets")]
        static IEnumerable<CodeInstruction> UpdateTargets_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Pop &&
                    codes[i - 1].opcode == OpCodes.Callvirt &&
                    codes[i + 1].opcode == OpCodes.Ldarg_0 &&
                    codes[i - 2].opcode == OpCodes.Ldloc_3)
                {
                    codes.InsertRange(i + 1, new CodeInstruction[] {
                        new CodeInstruction(OpCodes.Ldloc_S, 11),
                        new CodeInstruction(OpCodes.Ldloc_3),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DroneManager), "BroadcastDroneOrder", new System.Type[] { typeof(int), typeof(int) }))
                        });
                    break;
                }
            }
            return codes;
        }
    }
}
