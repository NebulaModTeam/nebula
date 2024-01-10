using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;

namespace NebulaPatcher.Patches.Transpilers;

internal delegate bool RemoveExtraSpawnedDrones(ConstructionSystem _this, ref DroneComponent droneComponent);

internal delegate bool IsOwnerAMecha(int owner);

internal delegate bool OwnerAndIdMatchMecha(int id, int owner);
internal delegate bool AreDronesEnabled(bool droneEnabled, ref DroneComponent drone);

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
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        var matcher = new CodeMatcher(codeInstructions)
            .MatchForward(true,
                new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "RecycleDrone"),
                new CodeMatch(OpCodes.Br));

        if (matcher.IsInvalid)
        {
            Log.Error(
                "ConstructionSystem_Transpiler.UpdateDrones_Transpiler 1 failed. Mod version not compatible with game version.");
            return codeInstructions;
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
            return codeInstructions;
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
                HarmonyLib.Transpilers.EmitDelegate<RemoveExtraSpawnedDrones>(
                    (ConstructionSystem _this, ref DroneComponent droneComponent) =>
                    {
                        var droneIsOwnedByMecha = droneComponent.owner == 0;
                        if (!Multiplayer.IsActive || !Multiplayer.Session.LocalPlayer.IsClient || !droneIsOwnedByMecha)
                        {
                            return false;
                        }
                        var idleCount = _this.player.mecha.constructionModule.droneIdleCount;
                        var droneCount = _this.player.mecha.constructionModule.droneCount;
                        if (idleCount != droneCount)
                        {
                            return false;
                        }
                        _this.player.mecha.constructionModule.RecycleDrone(_this.factory, ref droneComponent);
                        _this.player.mecha.constructionModule.droneIdleCount--; // because the above call increases it by one.
                        return true;
                    }),
                new CodeInstruction(OpCodes.Brtrue, continueJump));

        return matcher.InstructionEnumeration();
    }

    // still update rendering of other player drones, make sure this happens here.
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ConstructionSystem.UpdateDrones))]
    public static IEnumerable<CodeInstruction> UpdateDrones_Transpiler2(IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        var matcher = new CodeMatcher(codeInstructions, generator);

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
            return codeInstructions;
        }

        // change: bool flag = ptr.owner == 0;
        // to: bool flag = ptr.owner <= 0;
        matcher
            .SetInstruction(new CodeInstruction(OpCodes.Pop)) // remove the pushed 0
            .Advance(1)
            .InsertAndAdvance(
                HarmonyLib.Transpilers.EmitDelegate<IsOwnerAMecha>(owner =>
                {
                    if (!Multiplayer.IsActive)
                    {
                        return owner == 0;
                    }
                    return
                        owner <= 0; // we set owner to negative values in ConstructionModuleComponent_Transpiler to mark drones from other players.
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
            return codeInstructions;
        }

        // change: if (constructionModuleComponent.id != ptr.owner || ptr2.id != ptr.craftId)
        // to: if (ptr2.id != ptr.craftId)
        matcher
            .InsertAndAdvance(
                HarmonyLib.Transpilers.EmitDelegate<OwnerAndIdMatchMecha>((id, owner) =>
                {
                    if (!Multiplayer.IsActive)
                    {
                        return
                            id != owner; // game does exit when id does not match owner, so we do too when multiplayer is inactive
                    }

                    return false; // this might be a bit too open but will render any drone regardless of the games checks.
                }));
        var jmpOut = matcher.Operand;
        matcher.SetInstruction(new CodeInstruction(OpCodes.Brtrue, jmpOut));

        // now we also need to make sure that drones of other players still get rendered when the local player has his drones disabled.
        // so set: bool droneEnabled = constructionModuleComponent.droneEnabled;
        // to: bool droneEnabled = ptr.owner < 0 || constructionModuleComponent.droneEnabled
        // but only when multiplayer is active ofc

        matcher
            .MatchForward(true,
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "droneEnabled"));

        if (matcher.IsInvalid)
        {
            Log.Error(
                "ConstructionSystem_Transpiler.UpdateDrones_Transpiler 5 failed. Mod version not compatible with game version.");
            return codeInstructions;
        }

        matcher
            .Advance(1)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldloc_S, 8),
                HarmonyLib.Transpilers.EmitDelegate<AreDronesEnabled>((bool droneEnabled, ref DroneComponent drone) =>
                {
                    if (!Multiplayer.IsActive)
                    {
                        return droneEnabled;
                    }

                    return droneEnabled || drone.owner < 0;
                }));

        return matcher.InstructionEnumeration();
    }
}
