#region

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(DFGBaseComponent))]
internal class DFGBaseComponent_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(DFGBaseComponent.UpdateFactoryThreat))]
    public static IEnumerable<CodeInstruction> UpdateFactoryThreat_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            /*  Launch assault for player who on remote planet and has no power buildings
            from:
				if (num24 == 0.0 && this.groundSystem.local_player_grounded_alive)
                {
					num24 = 10.0;
					ref Vector3 ptr2 = ref this.groundSystem.local_player_pos;
					vector = new Vector3(ptr2.x - num, ptr2.y - num2, ptr2.z - num3);
					num18 = num16;
					num19 = num17;
                }
            to:
			>>	if (num24 == 0.0 && LaunchCondition(this))
                {
                    ...
                }
            */

            var codeMatcher = new CodeMatcher(instructions)
                .End()
                .MatchBack(true,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldc_R8),
                    new CodeMatch(OpCodes.Bne_Un),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(DFGBaseComponent), nameof(DFGBaseComponent.groundSystem))),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(EnemyDFGroundSystem), nameof(EnemyDFGroundSystem.local_player_grounded_alive))),
                    new CodeMatch(OpCodes.Brfalse));

            if (codeMatcher.IsInvalid)
            {
                Log.Warn("DFGBaseComponent.UpdateFactoryThreat: Can't find target");
                return codeMatcher.InstructionEnumeration();
            }
            codeMatcher.Advance(-2)
                .RemoveInstruction()
                .SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(DFGBaseComponent_Transpiler), nameof(LaunchCondition)));

            return codeMatcher.InstructionEnumeration();
        }
        catch (System.Exception e)
        {
            Log.Warn("Transpiler DFGBaseComponent.UpdateFactoryThreat failed.");
            Log.Warn(e);
            return instructions;
        }
    }

    static bool LaunchCondition(DFGBaseComponent @this)
    {
        if (!Multiplayer.IsActive) return @this.groundSystem.local_player_grounded_alive;

        // In MP, replace local_player_grounded_alive flag with the following condition
        var planetId = @this.groundSystem.planet.id;
        var players = Multiplayer.Session.Combat.Players;
        for (var i = 0; i < players.Length; i++)
        {
            if (players[i].isAlive && players[i].planetId == planetId)
            {
                @this.groundSystem.local_player_pos = players[i].position;
                Log.Info($"Base attack LaunchCondition: player[{i}] planeId{planetId}");
                return true;
            }
        }
        return false;
    }
}
