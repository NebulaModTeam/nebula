using HarmonyLib;
using NebulaModel.DataStructures;
using NebulaWorld;
using NebulaWorld.Factory;
using System;
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
    class CargoTraffic_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch("PickupBeltItems")]
        static IEnumerable<CodeInstruction> PickupBeltItems_Transpiler(ILGenerator gen, IEnumerable<CodeInstruction> instructions)
        {
            var found = false;
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
                    found = true;
                    codes.InsertRange(i + 1, new CodeInstruction[] {
                            new CodeInstruction(OpCodes.Ldloc_3),
                            new CodeInstruction(OpCodes.Ldloc_S, 4),
                            new CodeInstruction(OpCodes.Ldarg_2),
                            new CodeInstruction(OpCodes.Ldloc_2),
                            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BeltManager), "RegisterBeltPickupUpdate", new System.Type[] { typeof(int), typeof(int), typeof(int), typeof(int)})),
                    });
                    break;
                }
            }

            if (!found)
                NebulaModel.Logger.Log.Error("PickupBeltItems transpiler failed");

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
            var found = false;
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
                    found = true;
                    codes.InsertRange(i, new CodeInstruction[] {
                                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(FactoryManager), "DoNotAddItemsFromBuildingOnDestruct")),
                                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ToggleSwitch), "op_Implicit")),
                                    new CodeInstruction(OpCodes.Brtrue_S, codes[i-1].operand),
                                    });
                    break;
                }
            }

            if (!found)
                NebulaModel.Logger.Log.Error("AlterBeltConnections transpiler failed");

            return codes;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(CargoTraffic), nameof(CargoTraffic.CreateRenderingBatches))]
        [HarmonyPatch(typeof(CargoTraffic), nameof(CargoTraffic.AlterBeltConnections))]
        static IEnumerable<CodeInstruction> IsPlanetPhysicsColliderDirty_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var found = false;
            var code = new CodeMatcher(instructions, il)
                   .MatchForward(false,
                   new CodeMatch(OpCodes.Ldarg_0),
                   new CodeMatch(i => i.opcode == OpCodes.Ldfld && i.operand?.ToString() == "PlanetData planet"),
                   new CodeMatch(i => i.opcode == OpCodes.Ldfld && i.operand?.ToString() == "PlanetPhysics physics"),
                   new CodeMatch(OpCodes.Ldc_I4_1),
                   new CodeMatch(i => i.opcode == OpCodes.Stfld && i.operand?.ToString() == "System.Boolean isPlanetPhysicsColliderDirty"))
               .Repeat(matcher =>
               {
                   found = true;
                   matcher
                   .CreateLabelAt(matcher.Pos + 5, out Label end)
                   .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<bool>>(() =>
                   {
                       return FactoryManager.EventFromClient && LocalPlayer.IsMasterClient;
                   }))
                   .Insert(new CodeInstruction(OpCodes.Brtrue, end))
                   .Advance(5);
               })
               .InstructionEnumeration();

            if (!found)
                NebulaModel.Logger.Log.Error("IsPlanetPhysicsColliderDirty_Transpiler failed");

            return code;
        }
    }
}
