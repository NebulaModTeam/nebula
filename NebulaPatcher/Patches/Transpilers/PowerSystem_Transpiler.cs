using HarmonyLib;
using NebulaModel.Packets.Factory.PowerTower;
using NebulaWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpilers
{
    [HarmonyPatch(typeof(PowerSystem))]
    internal class PowerSystem_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(PowerSystem.RequestDysonSpherePower))]
        public static IEnumerable<CodeInstruction> PowerSystem_RequestDysonSpherePower_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //Prevent dysonSphere.energyReqCurrentTick from changing on the client side
            //Change: if (this.dysonSphere != null)
            //To:     if (this.dysonSphere != null && (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost))
            try
            {
                CodeMatcher codeMatcher = new CodeMatcher(instructions)
                    .MatchForward(true,
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerSystem), "dysonSphere")),
                        new CodeMatch(OpCodes.Brfalse) //IL #93
                    );
                object label = codeMatcher.Instruction.operand;
                codeMatcher.Advance(1)
                    .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<bool>>(() =>
                    {
                        return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost;
                    }))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse_S, label));
                return codeMatcher.InstructionEnumeration();
            }
            catch
            {
                NebulaModel.Logger.Log.Error("PowerSystem.RequestDysonSpherePower_Transpiler failed. Mod version not compatible with game version.");
                return instructions;
            }
        }
    }
}
