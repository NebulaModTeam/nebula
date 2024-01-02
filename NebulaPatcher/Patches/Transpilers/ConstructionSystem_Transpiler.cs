using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;
using UnityEngine.UI;
using UnityEngine.Yoga;

namespace NebulaPatcher.Patches.Transpilers
{
    delegate bool removeExtraSpawnedDrones(ConstructionSystem _this, ref DroneComponent droneComponent);
    delegate bool isOwnerAMecha(int owner);
    delegate bool ownerAndIdMatchMecha(int id, int owner);

    [HarmonyPatch(typeof(ConstructionSystem))]
    internal class ConstructionSystem_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(ConstructionSystem.UpdateDrones))]
        public static IEnumerable<CodeInstruction> UpdateDrones_Transpiler1(IEnumerable<CodeInstruction> instructions)
        {
            /*
             * Poor Fix: Clients spawn with the right count of construction drones but for some reason extra drones spawn somewhere on the planet and come back to the mecha,
             * draining core energy while doing so and increasing the drone count above the maximum when they arrive.
             * 
             * This patch aims to RecycleDrone() these to remove them as soon as possible.
             */

            // find the continue jmp to skip one iteration of the for loop in case we needed to RecycleDrone()
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "RecycleDrone"),
                    new CodeMatch(OpCodes.Br));

            if (matcher.IsInvalid)
            {
                Log.Error(
                    "ConstructionSystem_Transpiler.UpdateDrones_Transpiler 1 failed. Mod version not compatible with game version.");
                return instructions;
            }

            var continueJump = matcher.Operand;

            matcher
                .MatchBack(true,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "owner"),
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Ceq),
                    new CodeMatch(OpCodes.Stloc_S));

            if (matcher.IsInvalid)
            {
                Log.Error(
                    "ConstructionSystem_Transpiler.UpdateDrones_Transpiler 2 failed. Mod version not compatible with game version.");
                return instructions;
            }

            /*
             * from:
                ref DroneComponent ptr = ref this.drones.buffer[i];
			    if (ptr.id == i)
			    {
				    if (ptr.stage != 0)
				    {
					    bool flag = ptr.owner == 0;
					    ConstructionModuleComponent constructionModuleComponent = (flag ? this.player.mecha.constructionModule : this.constructionModules.buffer[ptr.owner]);
             * to:
                ref DroneComponent ptr = ref this.drones.buffer[i];
			    if (ptr.id == i)
			    {
				    if (ptr.stage != 0)
				    {
					    bool flag = ptr.owner == 0;
                        bool droneIsOwnedByMecha = ptr.owner == 0;
                        if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsClient && droneIsOwnedByMecha)
                        {
                            int idleCount = _this.player.mecha.constructionModule.droneIdleCount;
                            int droneCount = _this.player.mecha.constructionModule.droneCount;
                            if (idleCount == droneCount)
                            {
                                this.player.mecha.constructionModule.RecycleDrone(this.factory, ref ptr);
                                this.player.mecha.constructionModule.droneIdleCount--;
                                continue;
                            }
                        }
					    ConstructionModuleComponent constructionModuleComponent = (flag ? this.player.mecha.constructionModule : this.constructionModules.buffer[ptr.owner]);
             */
            matcher
                .Advance(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldloc_S, 8),
                    HarmonyLib.Transpilers.EmitDelegate<removeExtraSpawnedDrones>((ConstructionSystem _this, ref DroneComponent droneComponent) =>
                    {
                        bool droneIsOwnedByMecha = droneComponent.owner == 0;
                        if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsClient && droneIsOwnedByMecha)
                        {
                            int idleCount = _this.player.mecha.constructionModule.droneIdleCount;
                            int droneCount = _this.player.mecha.constructionModule.droneCount;
                            if (idleCount == droneCount)
                            {
                                _this.player.mecha.constructionModule.RecycleDrone(_this.factory, ref droneComponent);
                                _this.player.mecha.constructionModule.droneIdleCount--; // because the above call increases it by one.
                                return true;
                            }
                        }
                        return false;
                    }),
                    new CodeInstruction(OpCodes.Brtrue, continueJump));

            return matcher.InstructionEnumeration();
        }

        // still update rendering of other player drones, make sure this happens here.
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(ConstructionSystem.UpdateDrones))]
        public static IEnumerable<CodeInstruction> UpdateDrones_Transpiler2(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new CodeMatcher(instructions, generator);

            // flag must be true too if ptr.owner <= 0 as we set owner to negative values in ConstructionModuleComponent_Transpiler to mark drones from other players.
            matcher
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "owner"),
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Ceq));

            if (matcher.IsInvalid)
            {
                Log.Error(
                    "ConstructionSystem_Transpiler.UpdateDrones_Transpiler 3 failed. Mod version not compatible with game version.");
                return instructions;
            }

            matcher
                .SetInstruction(new CodeInstruction(OpCodes.Pop)) // remove the pushed 0
                .Advance(1)
                .InsertAndAdvance(
                    HarmonyLib.Transpilers.EmitDelegate<isOwnerAMecha>((int owner) =>
                    {
                        if (!Multiplayer.IsActive)
                        {
                            return owner == 0;
                        }
                        return owner <= 0; // we set owner to negative values in ConstructionModuleComponent_Transpiler to mark drones from other players.
                    }));
            matcher
            .MatchForward(true,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "id"),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "owner"),
                    new CodeMatch(OpCodes.Bne_Un));

            if (matcher.IsInvalid)
            {
                Log.Error(
                    "ConstructionSystem_Transpiler.UpdateDrones_Transpiler 4 failed. Mod version not compatible with game version.");
                return instructions;
            }

            matcher
                .InsertAndAdvance(
                    HarmonyLib.Transpilers.EmitDelegate<ownerAndIdMatchMecha>((int id, int owner) =>
                    {
                        if (!Multiplayer.IsActive)
                        {
                            return id != owner; // game does exit when id does not match owner, so we do too when multiplayer is inactive
                        }

                        return false; // we set owner to negative values in ConstructionModuleComponent_Transpiler to mark drones from other players. Those still need to be rendered/updated, as well as those from battle bases
                    }));
            var jmpOut = matcher.Operand;
            matcher.SetInstruction(new CodeInstruction(OpCodes.Brtrue, jmpOut));

            return matcher.InstructionEnumeration();
        }
    }
}
