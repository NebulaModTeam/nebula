using HarmonyLib;
using NebulaModel.Packets.Factory.PowerTower;
using NebulaWorld;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpilers
{
    [HarmonyPatch(typeof(PowerSystem))]
    class PowerSystem_Transpiler
    {
        delegate void PlayerChargesAtTower(PowerSystem _this, int powerNodeId, int powerNetId);

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(PowerSystem.GameTick))]
        public static IEnumerable<CodeInstruction> PowerSystem_GameTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerSystem), nameof(PowerSystem.nodePool))),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerSystem), nameof(PowerSystem.nodePool))),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerNodeComponent), nameof(PowerNodeComponent.workEnergyPerTick))),
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(PowerNodeComponent), nameof(PowerNodeComponent.requiredEnergy))))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 58))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 22))
                .Insert(HarmonyLib.Transpilers.EmitDelegate<PlayerChargesAtTower>((PowerSystem _this, int powerNodeId, int powerNetId) =>
                {
                    if (Multiplayer.IsActive)
                    {
                        if (!Multiplayer.Session.LocalPlayer.IsHost)
                        {
                            _this.nodePool[powerNodeId].requiredEnergy = _this.nodePool[powerNodeId].idleEnergyPerTick; // this gets added onto the known required energy by Multiplayer.Session.PowerTowers. and PowerSystem_Patch
                            if (Multiplayer.Session.PowerTowers.AddRequested(_this.planet.id, powerNetId, powerNodeId, true, false))
                            {
                                Multiplayer.Session.Network.SendPacket(new PowerTowerUserLoadingRequest(_this.planet.id, powerNetId, powerNodeId, _this.nodePool[powerNodeId].workEnergyPerTick, true));
                            }
                        }
                        else
                        {
                            PowerNetwork pNet = _this.netPool[powerNetId];
                            if (Multiplayer.Session.PowerTowers.AddRequested(_this.planet.id, powerNetId, powerNodeId, true, false))
                            {
                                Multiplayer.Session.PowerTowers.AddExtraDemand(_this.planet.id, powerNetId, powerNodeId, _this.nodePool[powerNodeId].workEnergyPerTick);
                                Multiplayer.Session.Network.SendPacketToLocalStar(new PowerTowerUserLoadingResponse(_this.planet.id, powerNetId, powerNodeId, _this.nodePool[powerNodeId].workEnergyPerTick,
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
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PowerNodeComponent), nameof(PowerNodeComponent.idleEnergyPerTick))),
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(PowerNodeComponent), nameof(PowerNodeComponent.requiredEnergy))))
                .Repeat(matcher =>
                {
                    matcher
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 58))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 22))
                        .Insert(HarmonyLib.Transpilers.EmitDelegate<PlayerChargesAtTower>((PowerSystem _this, int powerNodeId, int powerNetId) =>
                        {
                            if (Multiplayer.IsActive)
                            {
                                if (!Multiplayer.Session.LocalPlayer.IsHost)
                                {
                                    if (Multiplayer.Session.PowerTowers.AddRequested(_this.planet.id, powerNetId, powerNodeId, false, false))
                                    {
                                        Multiplayer.Session.Network.SendPacket(new PowerTowerUserLoadingRequest(_this.planet.id, powerNetId, powerNodeId, _this.nodePool[powerNodeId].workEnergyPerTick, false));
                                    }
                                }
                                else
                                {
                                    PowerNetwork pNet = _this.netPool[powerNetId];
                                    if (Multiplayer.Session.PowerTowers.AddRequested(_this.planet.id, powerNetId, powerNodeId, false, false))
                                    {
                                        Multiplayer.Session.PowerTowers.RemExtraDemand(_this.planet.id, powerNetId, powerNodeId);
                                        Multiplayer.Session.Network.SendPacketToLocalStar(new PowerTowerUserLoadingResponse(_this.planet.id, powerNetId, powerNodeId, _this.nodePool[powerNodeId].workEnergyPerTick,
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
