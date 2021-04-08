using HarmonyLib;
using NebulaWorld.Factory;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpiler
{
    [HarmonyPatch(typeof(PlayerAction_Build))]
    class PlayerAction_Build_Patch
    {
        [HarmonyTranspiler]
        [HarmonyPatch("CreatePrebuilds")]
        static IEnumerable<CodeInstruction> CreatePrebuilds_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            //Prevent spending items if user is not building
            //insert:  if (!FactoryManager.EventFromServer)
            //before check for resources to build
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].operand?.ToString() == "System.Int32 coverObjId" &&
                    codes[i + 1].opcode == OpCodes.Brfalse &&
                    codes[i + 3].operand?.ToString() == "System.Boolean willCover" &&
                    codes[i + 4].opcode == OpCodes.Brfalse &&
                    codes[i + 6].operand?.ToString() == "ItemProto item")
                {
                    Label targetLabel = (Label)codes[i + 4].operand;
                    codes.InsertRange(i - 1, new CodeInstruction[] {
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FactoryManager), "get_EventFromServer")),
                        new CodeInstruction(OpCodes.Brtrue_S, targetLabel)
                        });
                    break;
                }
            }
            return codes;
        }
    }
}
