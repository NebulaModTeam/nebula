#region

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory.PowerTower;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(PowerSystem))]
internal class PowerSystem_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PowerSystem.GameTick))]
    public static IEnumerable<CodeInstruction> PowerSystem_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codeMatcher = new CodeMatcher(instructions)
            .MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerSystem), nameof(PowerSystem.nodePool))),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldelema),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerSystem), nameof(PowerSystem.nodePool))),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldelema),
                new CodeMatch(OpCodes.Ldfld,
                    AccessTools.Field(typeof(PowerNodeComponent), nameof(PowerNodeComponent.workEnergyPerTick))),
                new CodeMatch(OpCodes.Stfld,
                    AccessTools.Field(typeof(PowerNodeComponent), nameof(PowerNodeComponent.requiredEnergy))));

        if (codeMatcher.IsInvalid)
        {
            Log.Error("PowerSystem_GameTick_Transpiler 1 failed. Mod version not compatible with game version.");
            return instructions;
        }

        codeMatcher = codeMatcher
            .Advance(1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 59))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 22))
            .Insert(HarmonyLib.Transpilers.EmitDelegate<PlayerChargesAtTower>((_this, powerNodeId, powerNetId) =>
            {
                if (Multiplayer.IsActive)
                {
                    if (!Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        _this.nodePool[powerNodeId].requiredEnergy =
                            _this.nodePool[powerNodeId]
                                .idleEnergyPerTick; // this gets added onto the known required energy by Multiplayer.Session.PowerTowers. and PowerSystem_Patch
                        if (Multiplayer.Session.PowerTowers.AddRequested(_this.planet.id, powerNetId, powerNodeId, true, false))
                        {
                            Multiplayer.Session.Network.SendPacket(new PowerTowerUserLoadingRequest(_this.planet.id, powerNetId,
                                powerNodeId, _this.nodePool[powerNodeId].workEnergyPerTick, true));
                        }
                    }
                    else
                    {
                        var pNet = _this.netPool[powerNetId];
                        if (Multiplayer.Session.PowerTowers.AddRequested(_this.planet.id, powerNetId, powerNodeId, true, false))
                        {
                            Multiplayer.Session.PowerTowers.AddExtraDemand(_this.planet.id, powerNetId, powerNodeId,
                                _this.nodePool[powerNodeId].workEnergyPerTick);
                            Multiplayer.Session.Network.SendPacketToLocalStar(new PowerTowerUserLoadingResponse(_this.planet.id,
                                powerNetId, powerNodeId, _this.nodePool[powerNodeId].workEnergyPerTick,
                                pNet.energyCapacity,
                                pNet.energyRequired,
                                pNet.energyServed,
                                pNet.energyAccumulated,
                                pNet.energyExchanged,
                                true));
                        }
                    }
                }
            }))
            // now search for where its set back to idle after player leaves radius / has charged fully
            .MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerSystem), nameof(PowerSystem.nodePool))),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldelema),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerSystem), nameof(PowerSystem.nodePool))),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldelema),
                new CodeMatch(OpCodes.Ldfld,
                    AccessTools.Field(typeof(PowerNodeComponent), nameof(PowerNodeComponent.idleEnergyPerTick))),
                new CodeMatch(OpCodes.Stfld,
                    AccessTools.Field(typeof(PowerNodeComponent), nameof(PowerNodeComponent.requiredEnergy))));

        if (codeMatcher.IsInvalid)
        {
            Log.Error("PowerSystem_GameTick_Transpiler 2 failed. Mod version not compatible with game version.");
            return codeMatcher.InstructionEnumeration();
        }

        return codeMatcher
            .Repeat(matcher =>
            {
                matcher
                    .Advance(1)
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 59))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 22))
                    .Insert(HarmonyLib.Transpilers.EmitDelegate<PlayerChargesAtTower>((_this, powerNodeId, powerNetId) =>
                    {
                        if (Multiplayer.IsActive)
                        {
                            if (!Multiplayer.Session.LocalPlayer.IsHost)
                            {
                                if (Multiplayer.Session.PowerTowers.AddRequested(_this.planet.id, powerNetId, powerNodeId,
                                        false, false))
                                {
                                    Multiplayer.Session.Network.SendPacket(new PowerTowerUserLoadingRequest(_this.planet.id,
                                        powerNetId, powerNodeId, _this.nodePool[powerNodeId].workEnergyPerTick, false));
                                }
                            }
                            else
                            {
                                var pNet = _this.netPool[powerNetId];
                                if (Multiplayer.Session.PowerTowers.AddRequested(_this.planet.id, powerNetId, powerNodeId,
                                        false, false))
                                {
                                    Multiplayer.Session.PowerTowers.RemExtraDemand(_this.planet.id, powerNetId, powerNodeId);
                                    Multiplayer.Session.Network.SendPacketToLocalStar(new PowerTowerUserLoadingResponse(
                                        _this.planet.id, powerNetId, powerNodeId, _this.nodePool[powerNodeId].workEnergyPerTick,
                                        pNet.energyCapacity,
                                        pNet.energyRequired,
                                        pNet.energyServed,
                                        pNet.energyAccumulated,
                                        pNet.energyExchanged,
                                        false));
                                }
                            }
                        }
                    }));
            })
            .InstructionEnumeration();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PowerSystem.RequestDysonSpherePower))]
    public static IEnumerable<CodeInstruction> PowerSystem_RequestDysonSpherePower_Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        //Prevent dysonSphere.energyReqCurrentTick from changing on the client side
        //Change: if (this.dysonSphere != null)
        //To:     if (this.dysonSphere != null && (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost))
        try
        {
            var codeMatcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerSystem), "dysonSphere")),
                    new CodeMatch(OpCodes.Brfalse) //IL #93
                );
            var label = codeMatcher.Instruction.operand;
            codeMatcher.Advance(1)
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate(() =>
                {
                    return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost;
                }))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse_S, label));
            return codeMatcher.InstructionEnumeration();
        }
        catch
        {
            Log.Error("PowerSystem.RequestDysonSpherePower_Transpiler failed. Mod version not compatible with game version.");
            return instructions;
        }
    }

    private delegate void PlayerChargesAtTower(PowerSystem _this, int powerNodeId, int powerNetId);
}
