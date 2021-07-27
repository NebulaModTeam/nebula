using HarmonyLib;
using NebulaModel.Packets.Factory.PowerTower;
using NebulaWorld;
using NebulaWorld.Factory;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpilers
{
    [HarmonyPatch(typeof(PowerSystem))]
    class PowerSystem_Transpiler
    {
        delegate void PlayerChargesAtTower(PowerSystem _this, int powerNodeId, int powerNetId);

        [HarmonyTranspiler]
        [HarmonyPatch("GameTick")]
        public static IEnumerable<CodeInstruction> PowerSystem_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerSystem), "nodePool")),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerSystem), "nodePool")),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerNodeComponent), "workEnergyPerTick")),
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(PowerNodeComponent), "requiredEnergy")))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 58))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 22))
                .Insert(HarmonyLib.Transpilers.EmitDelegate<PlayerChargesAtTower>((PowerSystem _this, int powerNodeId, int powerNetId) =>
                {
                    if (SimulatedWorld.Initialized)
                    {
                        if (!LocalPlayer.IsMasterClient)
                        {
                            _this.nodePool[powerNodeId].requiredEnergy = _this.nodePool[powerNodeId].idleEnergyPerTick; // this gets added onto the known required energy by PowerTowerManager and PowerSystem_Patch
                            if (PowerTowerManager.AddRequested(_this.planet.id, powerNetId, powerNodeId, true, false))
                            {
                                LocalPlayer.SendPacket(new PowerTowerUserLoadingRequest(_this.planet.id, powerNetId, powerNodeId, _this.nodePool[powerNodeId].workEnergyPerTick, true));
                            }
                        }
                        else
                        {
                            PowerNetwork pNet = _this.netPool[powerNetId];
                            if (PowerTowerManager.AddRequested(_this.planet.id, powerNetId, powerNodeId, true, false))
                            {
                                PowerTowerManager.AddExtraDemand(_this.planet.id, powerNetId, powerNodeId, _this.nodePool[powerNodeId].workEnergyPerTick);
                                LocalPlayer.SendPacketToLocalStar(new PowerTowerUserLoadingResponse(_this.planet.id, powerNetId, powerNodeId, _this.nodePool[powerNodeId].workEnergyPerTick,
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
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerSystem), "nodePool")),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerSystem), "nodePool")),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerNodeComponent), "idleEnergyPerTick")),
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(PowerNodeComponent), "requiredEnergy")))
                .Repeat(matcher =>
                {
                    matcher
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 58))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 22))
                        .Insert(HarmonyLib.Transpilers.EmitDelegate<PlayerChargesAtTower>((PowerSystem _this, int powerNodeId, int powerNetId) =>
                        {
                            if (SimulatedWorld.Initialized)
                            {
                                if (!LocalPlayer.IsMasterClient)
                                {
                                    if (PowerTowerManager.AddRequested(_this.planet.id, powerNetId, powerNodeId, false, false))
                                    {
                                        LocalPlayer.SendPacket(new PowerTowerUserLoadingRequest(_this.planet.id, powerNetId, powerNodeId, _this.nodePool[powerNodeId].workEnergyPerTick, false));
                                    }
                                }
                                else
                                {
                                    PowerNetwork pNet = _this.netPool[powerNetId];
                                    if (PowerTowerManager.AddRequested(_this.planet.id, powerNetId, powerNodeId, false, false))
                                    {
                                        PowerTowerManager.RemExtraDemand(_this.planet.id, powerNetId, powerNodeId);
                                        LocalPlayer.SendPacketToLocalStar(new PowerTowerUserLoadingResponse(_this.planet.id, powerNetId, powerNodeId, _this.nodePool[powerNodeId].workEnergyPerTick,
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

            return instructions;
        }
    }
}
