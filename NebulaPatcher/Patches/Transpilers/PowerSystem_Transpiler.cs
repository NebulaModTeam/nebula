#region

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory.PowerTower;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(PowerSystem))]
internal class PowerSystem_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PowerSystem.GameTick))]
    public static IEnumerable<CodeInstruction> PowerSystem_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
    {
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();

        try
        {
            /*  Get the variable of mecha power coefficient:
             	lock (mecha)
		        {
			        num7 = Mathf.Pow(Mathf.Clamp01((float)(1.0 - mainPlayer.mecha.coreEnergy / mainPlayer.mecha.coreEnergyCap) * 10f), 0.75f);
		        }
            */
            var codeMatcher = new CodeMatcher(codeInstructions, iLGenerator)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Mathf), nameof(Mathf.Pow))),
                    new CodeMatch(OpCodes.Stloc_S),
                    new CodeMatch(OpCodes.Leave));
            var coreEnergyRatioCI = new CodeInstruction(OpCodes.Ldloc_S, codeMatcher.InstructionAt(-1).operand);

            /* Overwrite the logic that set the power charger requiredEnergy and replace with our own.
            from:
                if (this.nodePool[id].id == id && this.nodePool[id].isCharger)
                {
                    if (this.nodePool[id].coverRadius <= 20f)
                    {
                        ...
                    }
                    else
                    {
                        this.nodePool[id].requiredEnergy = this.nodePool[id].idleEnergyPerTick;
                    }
                    long num21 = (long)this.nodePool[id].requiredEnergy;
                    num11 += num21;
                    num2 += num21;
                }
            to:
                if (this.nodePool[id].id == id && this.nodePool[id].isCharger)
                {
                    if (this.nodePool[id].coverRadius <= 20f)
                    {			        
                        SetChargerRequriePower(this, id, num7); //replace
                    }
                    else
                    {
                        this.nodePool[id].requiredEnergy = this.nodePool[id].idleEnergyPerTick;
                    }
                    long num21 = (long)this.nodePool[id].requiredEnergy;
                    num11 += num21;
                    num2 += num21;
                }
            */
            codeMatcher
                .MatchForward(false, new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "MoveNext"))
                .MatchBack(true,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerNodeComponent), nameof(PowerNodeComponent.idleEnergyPerTick))),
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(PowerNodeComponent), nameof(PowerNodeComponent.requiredEnergy))),
                    new CodeMatch(OpCodes.Ldarg_0));
            if (codeMatcher.IsInvalid)
            {
                Log.Error("PowerSystem_GameTick_Transpiler 1 failed. Mod version not compatible with game version.");
                return codeInstructions;
            }
            codeMatcher.CreateLabel(out var label);
            var nodeIdCI = codeMatcher.InstructionAt(-4);

            codeMatcher
                .MatchBack(true, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerNodeComponent), nameof(PowerNodeComponent.isCharger))))
                .MatchForward(true, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerNodeComponent), nameof(PowerNodeComponent.coverRadius))));
            if (codeMatcher.IsInvalid)
            {
                Log.Error("PowerSystem_GameTick_Transpiler 2 failed. Mod version not compatible with game version.");
                return codeInstructions;
            }
            codeMatcher.Advance(3)
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    nodeIdCI,
                    coreEnergyRatioCI,
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PowerSystem_Transpiler), nameof(SetChargerRequiredPower))),
                    new CodeInstruction(OpCodes.Br_S, label)
                );

            // Check if chargers are local before adding the energy to the mecha
            // from: if (this.nodePool[num71].id == num71)
            // get:  num71 (nodeId)
            codeMatcher.End()
                .MatchBack(true,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerNodeComponent), nameof(PowerNodeComponent.id))),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Bne_Un));
            if (codeMatcher.IsInvalid)
            {
                Log.Error("PowerSystem_GameTick_Transpiler 3 failed. Mod version not compatible with game version.");
                return codeInstructions;
            }
            var loadNodeIdInstruction = codeMatcher.InstructionAt(-1);

            /* A transpiler bug in BepInEx 5.4.22 will break charging part
               so the whole part need to handle early to fix the issue
            from:
				if (num73 <= 0 || entityAnimPool[entityId5].state != 2U)
				{
					goto IL_19FA;
				}
                ...
            to:
            	if (num77 <= 0 || entityAnimPool[entityId5].state != 2U)
				{
					goto IL_19FA;
				}
                AddMechaEnergy(this, num75) //Handle mecha energy increae here to not go to broken code part
                goto IL_19FA;
                ...
            */
            codeMatcher.MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(AnimData), nameof(AnimData.state))),
                    new CodeMatch(OpCodes.Ldc_I4_2),
                    new CodeMatch(OpCodes.Bne_Un));
            if (codeMatcher.IsInvalid)
            {
                Log.Error("PowerSystem_GameTick_Transpiler 4 failed. Mod version not compatible with game version.");
                return codeInstructions;
            }
            var continueLoopInstruction = new CodeInstruction(OpCodes.Br_S, codeMatcher.Operand);

            codeMatcher
                .Advance(1)
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    loadNodeIdInstruction,
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PowerSystem_Transpiler), nameof(AddMechaEnergy))),
                    continueLoopInstruction
                );

            return codeMatcher.InstructionEnumeration();
        }
        catch (System.Exception e)
        {
            Log.Error("PowerSystem_GameTick_Transpiler failed. Power chargers will not be in synced.");
            Log.Error(e);
            return codeInstructions;
        }
    }

#pragma warning disable IDE0060 // Temporarily fix
    private static void AddMechaEnergy(PowerSystem powerSystem, int nodeId)
    {
        if (GameMain.mainPlayer.planetId != powerSystem.factory.planetId)
            return; //Not in local planet

        if (Multiplayer.IsActive && !Multiplayer.Session.PowerTowers.LocalChargerIds.Contains(nodeId))
            return; //In MP: this charger is used by other players

        var mecha = GameMain.mainPlayer.mecha;
        ref var powerNode = ref powerSystem.nodePool[nodeId];
        var energyCharged = (int)((powerNode.requiredEnergy - powerNode.idleEnergyPerTick) * powerSystem.networkServes[powerNode.networkId]);
        lock (mecha)
        {
            mecha.coreEnergy += energyCharged;
            mecha.MarkEnergyChange(2, energyCharged);
            mecha.AddChargerDevice(powerNode.entityId);
            if (mecha.coreEnergy > mecha.coreEnergyCap)
            {
                mecha.coreEnergy = mecha.coreEnergyCap;
            }
        }
    }
#pragma warning restore IDE0060

#pragma warning disable CA1868
    private static void SetChargerRequiredPower(PowerSystem powerSystem, int nodeId, float coreEnergyRatio)
    {
        ref var powerNode = ref powerSystem.nodePool[nodeId];
        var planetId = powerSystem.factory.planetId;
        var isLocalPlanet = GameMain.mainPlayer.planetId == planetId;

        if (isLocalPlanet)
        {
            // Assume the game is multithread
            var dist = Vector3.SqrMagnitude(powerNode.powerPoint - powerSystem.multithreadPlayerPos);
            // vanilla code to make wireless charger range bigger
            var maxDist = (powerNode.coverRadius + 2.01f) * (powerNode.coverRadius + 2.01f);

            if (dist <= maxDist && coreEnergyRatio > 0)
            {
                // Mecha in range and require energy
                if (Multiplayer.IsActive)
                {
                    if (!Multiplayer.Session.PowerTowers.LocalChargerIds.Contains(powerNode.id))
                    {
                        // If player start requesting power and the node id hasn't been record, broadcast to other players
                        Multiplayer.Session.PowerTowers.LocalChargerIds.Add(powerNode.id);
                        Multiplayer.Session.Network.SendPacketToLocalStar(new PowerTowerChargerUpdate(
                            powerSystem.factory.planetId,
                            powerNode.id,
                            true));
                    }
                }
                powerNode.requiredEnergy = powerNode.workEnergyPerTick;
            }
            else
            {
                if (Multiplayer.IsActive)
                {
                    if (powerNode.requiredEnergy > powerNode.idleEnergyPerTick && Multiplayer.Session.PowerTowers.LocalChargerIds.Contains(powerNode.id))
                    {
                        // If player stop requesting power and the node id has been record, broadcast to other players
                        Multiplayer.Session.PowerTowers.LocalChargerIds.Remove(powerNode.id);
                        Multiplayer.Session.Network.SendPacketToLocalStar(new PowerTowerChargerUpdate(
                            powerSystem.factory.planetId,
                            powerNode.id,
                            false));
                    }
                }
                powerNode.requiredEnergy = powerNode.idleEnergyPerTick;
            }
        }
        else
        {
            powerNode.requiredEnergy = powerNode.idleEnergyPerTick;
        }

        if (Multiplayer.IsActive)
        {
            var hashId = ((long)planetId << 32) | (long)powerNode.id;
            if (Multiplayer.Session.PowerTowers.RemoteChargerHashIds.ContainsKey(hashId))
            {
                // This charger is used by remote player
                powerNode.requiredEnergy = powerNode.workEnergyPerTick;
            }
        }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PowerSystem.RequestDysonSpherePower))]
    public static IEnumerable<CodeInstruction> PowerSystem_RequestDysonSpherePower_Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        //Prevent dysonSphere.energyReqCurrentTick from changing on the client side
        //Change: if (this.dysonSphere != null)
        //To:     if (this.dysonSphere != null && (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost))
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        try
        {
            var codeMatcher = new CodeMatcher(codeInstructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerSystem), "dysonSphere")),
                    new CodeMatch(OpCodes.Brfalse) //IL #93
                );
            var label = codeMatcher.Instruction.operand;
            codeMatcher.Advance(1)
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate(() =>
                    !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse_S, label));
            return codeMatcher.InstructionEnumeration();
        }
        catch
        {
            Log.Error("PowerSystem.RequestDysonSpherePower_Transpiler failed. Mod version not compatible with game version.");
            return codeInstructions;
        }
    }
}
