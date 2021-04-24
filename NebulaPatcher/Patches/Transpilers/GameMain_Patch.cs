using HarmonyLib;
using NebulaWorld;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpiler
{
    [HarmonyPatch(typeof(GameMain))]
    class GameMain_Patch
    {
        //Ignore Pausing in the multiplayer:
        //Change:  if (!this._paused)
        //To:      if (!this._paused || SimulatedWorld.Initialized)

        [HarmonyTranspiler] 
        [HarmonyPatch("FixedUpdate")]
        static IEnumerable<CodeInstruction> PickupBeltItems_Transpiler(ILGenerator gen, IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 6; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt &&
                    codes[i - 1].opcode == OpCodes.Ldc_I4_1 &&
                    codes[i - 2].opcode == OpCodes.Br &&
                    codes[i - 3].opcode == OpCodes.Ldc_I4_0 &&
                    codes[i - 4].opcode == OpCodes.Br &&
                    codes[i - 5].opcode == OpCodes.Ceq)
                {
                    //Define new jump for firct condition
                    Label targetLabel = gen.DefineLabel();
                    codes[i + 4].labels.Add(targetLabel);

                    //Add my condition
                    codes.InsertRange(i + 4, new CodeInstruction[] {
                            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SimulatedWorld), "get_Initialized")),
                            new CodeInstruction(OpCodes.Brfalse_S, codes[i+3].operand)
                    });

                    //Change jump of first condition
                    codes[i + 3] = new CodeInstruction(OpCodes.Brfalse_S, targetLabel);

                    break;
                }
            }
            return codes;
        }
    }
}
