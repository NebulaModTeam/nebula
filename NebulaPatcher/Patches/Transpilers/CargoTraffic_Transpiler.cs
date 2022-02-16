using HarmonyLib;
using NebulaWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpiler
{
    /*  
     *  Add:
     *      Multiplayer.Session.Belts.RegisterBeltPickupUpdate(itemId, count, beltId);
     *  After:
     *      int itemId = cargoPath.TryPickItem(i - 4 - 1, 12);
    */
    [HarmonyPatch(typeof(CargoTraffic))]
    internal class CargoTraffic_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(CargoTraffic.PickupBeltItems))]
        private static IEnumerable<CodeInstruction> PickupBeltItems_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iL)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions, iL)
                    .MatchForward(true,
                        new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == nameof(CargoPath.TryPickItem))
                    );

            if (codeMatcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("CargoTraffic.PickupBeltItems_Transpiler failed. Mod version not compatible with game version.");
                return instructions;
            }

            return codeMatcher
                    .Advance(2)
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 5))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_2))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_3))
                    .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Action<int, int, int, bool>>((item, cnt, belt, all) =>
                    {
                        // Only pickup by hand needs to be synced
                        if (Multiplayer.IsActive && !all)
                        {
                            Multiplayer.Session.Belts.RegisterBeltPickupUpdate(item, cnt, belt);
                        }
                    }))
                    .InstructionEnumeration();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(CargoTraffic), nameof(CargoTraffic.CreateRenderingBatches))]
        [HarmonyPatch(typeof(CargoTraffic), nameof(CargoTraffic.AlterBeltConnections))]
        private static IEnumerable<CodeInstruction> IsPlanetPhysicsColliderDirty_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions, il)
                   .MatchForward(false,
                   new CodeMatch(OpCodes.Ldarg_0),
                   new CodeMatch(i => i.opcode == OpCodes.Ldfld && i.operand?.ToString() == "PlanetData planet"),
                   new CodeMatch(i => i.opcode == OpCodes.Ldfld && i.operand?.ToString() == "PlanetPhysics physics"),
                   new CodeMatch(OpCodes.Ldc_I4_1),
                   new CodeMatch(i => i.opcode == OpCodes.Stfld && i.operand?.ToString() == "System.Boolean isPlanetPhysicsColliderDirty"));

            if (codeMatcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("CargoTraffic_IsPlanetPhysicsColliderDirty_Transpiler failed. Mod version not compatible with game version.");
                return instructions;
            }

            return codeMatcher
                       .Repeat(matcher =>
                       {
                           matcher
                           .CreateLabelAt(matcher.Pos + 5, out Label end)
                           .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<bool>>(() =>
                           {
                               return Multiplayer.IsActive && Multiplayer.Session.Factories.IsIncomingRequest.Value;
                           }))
                           .Insert(new CodeInstruction(OpCodes.Brtrue, end))
                           .Advance(5);
                       })
                       .InstructionEnumeration();
        }
    }
}
