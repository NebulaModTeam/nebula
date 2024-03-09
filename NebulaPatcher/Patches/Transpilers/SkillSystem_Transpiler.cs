#region

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(SkillSystem))]
internal class SkillSystem_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(SkillSystem.DamageGroundObjectByLocalCaster))]
    [HarmonyPatch(nameof(SkillSystem.DamageGroundObjectByRemoteCaster))]
    public static IEnumerable<CodeInstruction> DamageGroundObject_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            /*  Overwrite player id 1 to PlayerId
            from:
                else if (target.type == ETargetType.Player && target.id == 1)
            to:
                else if (target.type == ETargetType.Player && target.id == NebulaWorld.Combat.CombatManager.PlayerId)
            */

            var codeMatcher = new CodeMatcher(instructions)
                .End()
                .MatchBack(true,
                    new CodeMatch(i => i.IsLdarg()),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(SkillTargetLocal), nameof(SkillTargetLocal.id))),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Bne_Un)
                )
                .Advance(-1)
                .Set(OpCodes.Call, AccessTools.DeclaredPropertyGetter(typeof(NebulaWorld.Combat.CombatManager),
                    nameof(NebulaWorld.Combat.CombatManager.PlayerId)));

            return codeMatcher.InstructionEnumeration();
        }
        catch (System.Exception e)
        {
            Log.Error("Transpiler SkillSystem.DamageGroundObject failed.");
            Log.Error(e);
            return instructions;
        }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(SkillSystem.DamageObject))]
    public static IEnumerable<CodeInstruction> DamageObject_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            /*  Overwrite player id 1 to PlayerId
            from:
                if (target.id == 1)
            to:
                if (target.id == NebulaWorld.Combat.CombatManager.PlayerId)
            */

            var codeMatcher = new CodeMatcher(instructions)
                .End()
                .MatchBack(true,
                    new CodeMatch(i => i.IsLdarg()),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(SkillTarget), nameof(SkillTarget.id))),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Bne_Un)
                )
                .Advance(-1)
                .Set(OpCodes.Call, AccessTools.DeclaredPropertyGetter(typeof(NebulaWorld.Combat.CombatManager),
                    nameof(NebulaWorld.Combat.CombatManager.PlayerId)));

            return codeMatcher.InstructionEnumeration();
        }
        catch (System.Exception e)
        {
            Log.Error("Transpiler SkillSystem.DamageObject failed.");
            Log.Error(e);
            return instructions;
        }
    }
}
