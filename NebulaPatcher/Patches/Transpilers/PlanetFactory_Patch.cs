using HarmonyLib;
using NebulaModel.DataStructures;
using NebulaWorld;
using NebulaWorld.Factory;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpiler
{
    [HarmonyPatch(typeof(PlanetFactory))]
    class PlanetFactory_Patch
    {
        /* Change:
             this.TakeBackItemsInEntity(player, objId);
         * 
         * To:
            if (!FactoryManager.DoNotAddItemsFromBuildingOnDestruct) {
			    this.TakeBackItemsInEntity(player, objId);
			}
         */
        [HarmonyTranspiler]
        [HarmonyPatch("DestructFinally")]
        static IEnumerable<CodeInstruction> DestructFinally_Transpiler(ILGenerator gen, IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand.ToString() == "Void NotifyObjectDestruct(EObjectType, Int32)" &&
                    codes[i - 1].opcode == OpCodes.Ldloc_0 &&
                    codes[i - 2].opcode == OpCodes.Ldc_I4_0 &&
                    codes[i - 3].opcode == OpCodes.Ldfld)
                {
                    Label targetLabel = gen.DefineLabel();
                    codes[i + 5].labels.Add(targetLabel);

                    codes.InsertRange(i + 1, new CodeInstruction[] {
                                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(FactoryManager), "DoNotAddItemsFromBuildingOnDestruct")),
                                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ToggleSwitch), "op_Implicit")),
                                    new CodeInstruction(OpCodes.Brtrue_S, targetLabel),
                                    });
                    break;
                }
            }
            return codes;
        }

        [HarmonyTranspiler]
        [HarmonyPatch("OnBeltBuilt")]
        static IEnumerable<CodeInstruction> OnBeltBuilt_Transpiler(ILGenerator gen, IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && ((MethodInfo)codes[i].operand).Name == "SetInserterPickTarget" &&
                    codes[i - 1].opcode == OpCodes.Sub &&
                    codes[i - 2].opcode == OpCodes.Ldloc_S &&
                    codes[i - 3].opcode == OpCodes.Ldloc_S)
                {
                    codes.InsertRange(i + 1, new CodeInstruction[] {
                                    new CodeInstruction(OpCodes.Ldloc_S, 9),
                                    new CodeInstruction(OpCodes.Ldloc_S, 21),
                                    new CodeInstruction(OpCodes.Ldloc_S, 10),
                                    new CodeInstruction(OpCodes.Ldloc_S, 16),
                                    new CodeInstruction(OpCodes.Ldloc_S, 22),
                                    new CodeInstruction(OpCodes.Sub),
                                    new CodeInstruction(OpCodes.Ldloc_S, 4),
                                    new CodeInstruction(OpCodes.Ldloc_S, 16),
                                    new CodeInstruction(OpCodes.Ldelem, typeof(UnityEngine.Vector3)),
                                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FactoryManager), "OnNewSetInserterPickTarget")),
                                    });
                    break;
                }
            }

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && ((MethodInfo)codes[i].operand).Name == "SetInserterPickTarget" &&
                    codes[i - 1].opcode == OpCodes.Sub &&
                    codes[i - 2].opcode == OpCodes.Ldloc_S &&
                    codes[i - 3].opcode == OpCodes.Ldloc_S)
                {
                    codes.InsertRange(i + 1, new CodeInstruction[] {
                                    new CodeInstruction(OpCodes.Ldloc_S, 9),
                                    new CodeInstruction(OpCodes.Ldloc_S, 30),
                                    new CodeInstruction(OpCodes.Ldloc_S, 10),
                                    new CodeInstruction(OpCodes.Ldloc_S, 16),
                                    new CodeInstruction(OpCodes.Ldloc_S, 31),
                                    new CodeInstruction(OpCodes.Sub),
                                    new CodeInstruction(OpCodes.Ldloc_S, 4),
                                    new CodeInstruction(OpCodes.Ldloc_S, 16),
                                    new CodeInstruction(OpCodes.Ldelem, typeof(UnityEngine.Vector3)),
                                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FactoryManager), "OnNewSetInserterInsertTarget")),
                                    });
                    break;
                }
            }
            return codes;
        }
    }
}
