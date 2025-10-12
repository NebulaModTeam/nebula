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
    //TODO: Fix the endless loop in this transpiler
    //[HarmonyTranspiler]
    //[HarmonyPatch(nameof(PowerSystem.GameTick))]
    public static IEnumerable<CodeInstruction> PowerSystem_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
    {
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();

        try
        {
            /*  Get the variable of mecha power coefficient:
             	lock (mecha2)
		        {
			        num9 = Mathf.Pow(Mathf.Clamp01((float)(1.0 - mecha.coreEnergy / mecha.coreEnergyCap) * 10f), 0.75f);
		        }
            */
            var codeMatcher = new CodeMatcher(codeInstructions, iLGenerator)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Mathf), nameof(Mathf.Pow))),
                    new CodeMatch(OpCodes.Stloc_S),
                    new CodeMatch(OpCodes.Leave));
            if (codeMatcher.IsInvalid)
            {
                Log.Error("PowerSystem_GameTick_Transpiler 0 failed. Power chargers will not be in synced.");
                return codeInstructions;
            }
            var coreEnergyRatioCI = new CodeInstruction(OpCodes.Ldloc_S, codeMatcher.InstructionAt(-1).operand);

            /* Overwrite the logic that set the power charger requiredEnergy and replace with our own.
             * DeepProfiler Section: DPEntry.PowerNode
            from:
                if (ptr.id == id && ptr.isCharger)
                {
                    if (ptr.coverRadius <= 20f)
                    {
                        ...
                    }
                    else
                    {
                        ptr.requiredEnergy = ptr.idleEnergyPerTick;
                    }
					long num23 = (long)ptr.requiredEnergy;
					num13 += num23;
					num3 += num23;
                }
            to:
                if (ptr.id == id && ptr.isCharger)
                {
                    if (ptr.coverRadius <= 20f)
                    {			        
                        SetChargerRequriePower(this, id, num7); //replace
                        goto label;
                    }
                    else
                    {
                        ptr.requiredEnergy = ptr.idleEnergyPerTick;
                    }
					label: long num23 = (long)ptr.requiredEnergy; //add label
					num13 += num23;
					num3 += num23;
                }
            */
            codeMatcher
                .MatchForward(false, new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "MoveNext"))
                .MatchBack(true,
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == nameof(PowerNodeComponent.idleEnergyPerTick)),
                    new CodeMatch(i => i.opcode == OpCodes.Stfld && ((FieldInfo)i.operand).Name == nameof(PowerNodeComponent.requiredEnergy)),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == nameof(PowerNodeComponent.requiredEnergy)),
                    new CodeMatch(OpCodes.Conv_I8),
                    new CodeMatch(OpCodes.Stloc_S));
            if (codeMatcher.IsInvalid)
            {
                Log.Error("PowerSystem_GameTick_Transpiler 1 failed. Power chargers will not be in synced.");
                return codeInstructions;
            }
            var nodePtrCI = codeMatcher.InstructionAt(-3);
            var label = codeMatcher.Advance(-3).Labels.First();

            codeMatcher
                .MatchBack(true, new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == nameof(PowerNodeComponent.isCharger)))
                .MatchForward(true, new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == nameof(PowerNodeComponent.coverRadius)));
            if (codeMatcher.IsInvalid)
            {
                Log.Error("PowerSystem_GameTick_Transpiler 2 failed. Power chargers will not be in synced.");
                return codeInstructions;
            }
            codeMatcher.Advance(3)
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    nodePtrCI,
                    coreEnergyRatioCI,
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PowerSystem_Transpiler), nameof(SetChargerRequiredPower))),
                    new CodeInstruction(OpCodes.Br_S, label)
                );

            // Check if chargers are local before adding the energy to the mecha
            // from: if (this.nodePool[num76].id == num76)
            // get:  num76 (nodeId)
            codeMatcher.End()
                .MatchBack(true,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerNodeComponent), nameof(PowerNodeComponent.id))),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Bne_Un));
            if (codeMatcher.IsInvalid)
            {
                Log.Error("PowerSystem_GameTick_Transpiler 3 failed. Power chargers will not be in synced.");
                return codeInstructions;
            }
            var loadNodeIdInstruction = codeMatcher.InstructionAt(-1);

            /* A transpiler bug in BepInEx 5.4.22 will break charging part
               so the whole part need to handle early to fix the issue
            from:
				if (num78 <= 0 || entityAnimPool[entityId5].state != 2U)
				{
					goto IL_18E2;
				}
                ...
            to:
            	if (num78 <= 0 || entityAnimPool[entityId5].state != 2U)
				{
					goto IL_18E2;
				}
                AddMechaEnergy(this, num78) //Handle mecha energy increase here to not go to broken code part
                goto IL_18E2;
                ...
            */
            codeMatcher.MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(AnimData), nameof(AnimData.state))),
                    new CodeMatch(OpCodes.Ldc_I4_2),
                    new CodeMatch(OpCodes.Bne_Un));
            if (codeMatcher.IsInvalid)
            {
                Log.Error("PowerSystem_GameTick_Transpiler 4 failed. Power chargers will not be in synced.");
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
    private static void SetChargerRequiredPower(PowerSystem powerSystem, ref PowerNodeComponent ptr, float coreEnergyRatio)
    {
        ref var powerNode = ref powerSystem.nodePool[ptr.id];
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
        //Change: if (this.dysonSphere != null) dysonSphere.energyReqCurrentTick += num;
        //To:     if (this.dysonSphere != null && (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)) ...
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        try
        {
            var codeMatcher = new CodeMatcher(codeInstructions)
                .End()
                .MatchBack(true,
                    new CodeMatch(i => i.IsLdloc()),
                    new CodeMatch(OpCodes.Brfalse) //IL #66
                );
            var label = codeMatcher.Instruction.operand;
            codeMatcher.Advance(1)
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate(() =>
                    !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse_S, label));
            return codeMatcher.InstructionEnumeration();
        }
        catch (System.Exception e)
        {
            Log.Error("PowerSystem.RequestDysonSpherePower_Transpiler failed. Dyson power generation on client will be incorrect.");
            Log.Warn(e);
            return codeInstructions;
        }
    }
}
