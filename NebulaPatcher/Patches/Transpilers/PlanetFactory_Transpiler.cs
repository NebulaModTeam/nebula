using HarmonyLib;
using NebulaModel.DataStructures;
using NebulaWorld;
using NebulaWorld.Factory;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpiler
{
    [HarmonyPatch(typeof(PlanetFactory))]
    class PlanetFactory_Transpiler
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
        [HarmonyPatch(nameof(PlanetFactory.DismantleFinally))]
        static IEnumerable<CodeInstruction> DismantleFinally_Transpiler(ILGenerator gen, IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions, gen)
                .MatchForward(false,
                    new CodeMatch(i => i.IsLdarg()),
                    new CodeMatch(i => i.IsLdarg()),
                    new CodeMatch(i => i.IsLdloc()),
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "TakeBackItemsInEntity"));

                if(matcher.IsInvalid)
                    NebulaModel.Logger.Log.Error("PlanetFactory.DismantleFinally() Transpiler failed");

            return matcher.CreateLabelAt(matcher.Pos + 4, out Label label)
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<bool>>(() =>
                {
                    return FactoryManager.DoNotAddItemsFromBuildingOnDestruct;
                }))
                .Insert(new CodeInstruction(OpCodes.Brtrue, label)).InstructionEnumeration();
        }

        [HarmonyTranspiler]
        [HarmonyPatch("OnBeltBuilt")]
        static IEnumerable<CodeInstruction> OnBeltBuilt_Transpiler(ILGenerator gen, IEnumerable<CodeInstruction> instructions)
        {
            var found = false;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && ((MethodInfo)codes[i].operand).Name == "SetInserterPickTarget" &&
                    codes[i - 1].opcode == OpCodes.Sub &&
                    codes[i - 2].opcode == OpCodes.Ldloc_S &&
                    codes[i - 3].opcode == OpCodes.Ldloc_S)
                {
                    found = true;
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

            if(!found)
                NebulaModel.Logger.Log.Error("OnBeltBuilt transpiler 1 failed");

            found = false;

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && ((MethodInfo)codes[i].operand).Name == "SetInserterPickTarget" &&
                    codes[i - 1].opcode == OpCodes.Sub &&
                    codes[i - 2].opcode == OpCodes.Ldloc_S &&
                    codes[i - 3].opcode == OpCodes.Ldloc_S)
                {
                    found = true;
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

            if(!found)
                NebulaModel.Logger.Log.Error("OnBeltBuilt transpiler 2 failed");

            return codes;
        }
    }
}
