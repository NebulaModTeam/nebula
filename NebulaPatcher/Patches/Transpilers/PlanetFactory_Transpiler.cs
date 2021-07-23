using HarmonyLib;
using NebulaWorld.Factory;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpiler
{
    [HarmonyPatch(typeof(PlanetFactory))]
    class PlanetFactory_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(PlanetFactory.OnBeltBuilt))]
        static IEnumerable<CodeInstruction> OnBeltBuilt_Transpiler(IEnumerable<CodeInstruction> instructions)
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

            if (!found)
                NebulaModel.Logger.Log.Error("OnBeltBuilt transpiler 1 failed. Mod version not compatible with game version.");

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

            if (!found)
                NebulaModel.Logger.Log.Error("OnBeltBuilt transpiler 2 failed. Mod version not compatible with game version.");

            return codes;
        }
    }
}
