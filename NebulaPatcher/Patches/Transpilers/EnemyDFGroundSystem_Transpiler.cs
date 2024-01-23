#region

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(EnemyDFGroundSystem))]
internal class EnemyDFGroundSystem_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(EnemyDFGroundSystem.GameTickLogic))]
    public static IEnumerable<CodeInstruction> GameTickLogic_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            /*  Overwrite the condition of local player and position in MP
                First make it skips the original logic that only work for singleplayer, by setting local_player_exist = false
            from:
                if (isLocalLoaded) {
                    Player mainPlayer = this.gameData.mainPlayer;
			        this.local_player_pos = mainPlayer.position;
			        this.local_player_pos_lf = mainPlayer.position; // use by UpdateFactoryThreat, which will not execute if local_player_total_energy_consume = 0
			        this.local_player_exist = true;                 // only inside this function
			        this.local_player_alive = mainPlayer.isAlive;   // only inside this function
			        this.local_player_exist_alive = this.local_player_exist && this.local_player_alive;
			        this.local_player_grounded_alive = this.local_player_exist_alive && !mainPlayer.sailing;
			        this.local_player_total_energy_consume = (long)(mainPlayer.mecha.totalEnergyConsume + 0.5); // use in KeyTickLogic
                }
                else {
                    ...
                }
            to:
                if (isLocalLoaded && !Multiplayer.IsActive) {
                    Player mainPlayer = this.gameData.mainPlayer;
                    ...
                }
                else {
                    ...
                }
            */

            var codeMatcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Call, AccessTools.DeclaredPropertyGetter(typeof(EnemyDFGroundSystem), nameof(EnemyDFGroundSystem.isLocalLoaded))),
                    new CodeMatch(i => i.IsStloc()),
                    new CodeMatch(i => i.IsLdloc()),
                    new CodeMatch(OpCodes.Brfalse));

            var jumpOperand = codeMatcher.Instruction.operand;
            codeMatcher.Advance(1)
                .Insert(
                    new CodeInstruction(OpCodes.Call, AccessTools.DeclaredPropertyGetter(typeof(Multiplayer), nameof(Multiplayer.IsActive))),
                    new CodeInstruction(OpCodes.Brtrue_S, jumpOperand)
                );

            return codeMatcher.InstructionEnumeration();
        }
        catch (System.Exception e)
        {
            Log.Error("Transpiler EnemyDFGroundSystem.GameTickLogic failed. Ground DF untis aggro will not in sync.");
            Log.Error(e);
            return instructions;
        }
    }
}
