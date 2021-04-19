using HarmonyLib;
using NebulaModel.DataStructures;
using NebulaWorld.Factory;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpiler
{
    /* Change this:
          if (num3 > 0)
		    {
		        UIItemup.Up(itemId, num3);
		    }

     *  To this:
            if (num3 > 0)
		        {
                    BeltManager.RegisterBeltPickupUpdate(itemId, count, beltId, segId);
		            UIItemup.Up(itemId, num3);
		        }
    */
    [HarmonyPatch(typeof(CargoTraffic))]
    class CargoTraffic_Patch
    {
        [HarmonyTranspiler]
        [HarmonyPatch("PickupBeltItems")]
        static IEnumerable<CodeInstruction> PickupBeltItems_Transpiler(ILGenerator gen, IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ble &&
                    codes[i - 1].opcode == OpCodes.Ldc_I4_0 &&
                    codes[i - 2].opcode == OpCodes.Ldloc_S &&
                    codes[i - 3].opcode == OpCodes.Stloc_S &&
                    codes[i - 4].opcode == OpCodes.Callvirt &&
                    codes[i - 5].opcode == OpCodes.Ldfld)
                {
                    codes.InsertRange(i + 1, new CodeInstruction[] {
                            new CodeInstruction(OpCodes.Ldloc_S, 4),
                            new CodeInstruction(OpCodes.Ldloc_S, 5),
                            new CodeInstruction(OpCodes.Ldarg_2),
                            new CodeInstruction(OpCodes.Ldloc_3),
                            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BeltManager), "RegisterBeltPickupUpdate", new System.Type[] { typeof(int), typeof(int), typeof(int), typeof(int)})),
                    });
                    break;
                }
            }
            return codes;
        }

        /* Change:
                if (num27 > 0)
					{
						int upCount = GameMain.mainPlayer.TryAddItemToPackage(num27, 1, true, this.beltPool[beltId].entityId);
						UIItemup.Up(num27, upCount);
					}

         * To:
            if (num27 > 0 && !FactoryManager.DoNotAddItemsFromBuildingOnDestruct)
					{
						int upCount = GameMain.mainPlayer.TryAddItemToPackage(num27, 1, true, this.beltPool[beltId].entityId);
						UIItemup.Up(num27, upCount);
					}
         */
        [HarmonyTranspiler]
        [HarmonyPatch("AlterBeltConnections")]
        static IEnumerable<CodeInstruction> AlterBeltConnections_Transpiler(ILGenerator gen, IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call && codes[i].operand?.ToString() == "Player get_mainPlayer()" &&
                    codes[i - 1].opcode == OpCodes.Ble &&
                    codes[i - 2].opcode == OpCodes.Ldc_I4_0 &&
                    codes[i - 3].opcode == OpCodes.Ldloc_S &&
                    codes[i - 4].opcode == OpCodes.Stloc_S &&
                    codes[i - 5].opcode == OpCodes.Ldelem_I4 &&
                    codes[i - 6].opcode == OpCodes.Ldloc_S &&
                    codes[i - 7].opcode == OpCodes.Ldfld)
                {
                    codes.InsertRange(i, new CodeInstruction[] {
                                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(FactoryManager), "DoNotAddItemsFromBuildingOnDestruct")),
                                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ToggleSwitch), "op_Implicit")),
                                    new CodeInstruction(OpCodes.Brtrue_S, codes[i-1].operand),
                                    });
                    break;
                }
            }
            return codes;
        }
    }
}
