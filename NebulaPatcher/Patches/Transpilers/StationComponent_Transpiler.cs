using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using NebulaWorld;
using NebulaModel.Packets.Logistics;
using UnityEngine;

// thanks tanu and Therzok for the tipps!
namespace NebulaPatcher.Patches.Transpilers
{
    [HarmonyPatch(typeof(StationComponent))]
    class StationComponent_Transpiler
    {
        // desc of function to inject into InternalTickRemote after an AddItem() call
        delegate int ShipFunc(StationComponent stationComponent, ref ShipData shipData);
        delegate int RemOrderFunc(StationComponent stationComponent, ref SupplyDemandPair supplyDemandPair);
        delegate int RemOrderFunc2(StationComponent stationComponent, int index);

        private static int RemOrderCounter = 0;
        private static int RemOrderCounter2 = 0;

        [HarmonyTranspiler]
        [HarmonyPatch("InternalTickRemote")]
        public static IEnumerable<CodeInstruction> InternalTickRemote_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // BEGIN: transpilers to catch AddItem() and TakeItem()
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldc_R4),
                    new CodeMatch(OpCodes.Stfld),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "AddItem"),
                    new CodeMatch(OpCodes.Pop)) // inject before this IL line
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0)) // load this ptr (pointing to the current StationComponent we are working on)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 35)) // load the ShipData ptr of the current ship we are working on
            .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<ShipFunc>((StationComponent stationComponent, ref ShipData shipData) =>
            {
                if(SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                {
                    ILSShipItems packet = new ILSShipItems(true, shipData.itemId, shipData.itemCount, shipData.shipIndex, stationComponent.gid);
                    LocalPlayer.SendPacket(packet);
                }
                return 0;
            }))
            .Insert(new CodeInstruction(OpCodes.Pop)) // pop the loaded ShipData away
            .InstructionEnumeration();

            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Ble),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "AddItem"),
                    new CodeMatch(OpCodes.Pop))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 130)) // the other StationComponent ('StationComponent stationComponent3 = gStationPool[shipData.otherGId];')
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 35))
            .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<ShipFunc>((StationComponent stationComponent, ref ShipData shipData) =>
            {
                if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                {
                    ILSShipItems packet = new ILSShipItems(true, shipData.itemId, shipData.itemCount, shipData.shipIndex, stationComponent.gid);
                    LocalPlayer.SendPacket(packet);
                }
                return 0;
            }))
            .Insert(new CodeInstruction(OpCodes.Pop))
            .InstructionEnumeration();

            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_S),
                    new CodeMatch(OpCodes.Stloc_S),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "TakeItem"),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Stfld)) // insert after 'shipData.itemCount = num77;' so we can query the itemCount out of the ShipData in the delegate
            .Advance(1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 130))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 35))
            .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<ShipFunc>((StationComponent stationComponent, ref ShipData shipData) =>
            {
                if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                {
                    ILSShipItems packet = new ILSShipItems(false, shipData.itemId, shipData.itemCount, shipData.shipIndex, stationComponent.gid);
                    LocalPlayer.SendPacket(packet);
                }
                return 0;
            }))
            .Insert(new CodeInstruction(OpCodes.Pop))
            .InstructionEnumeration();
            // END: transpilers to catch AddItem() and TakeItem()

            // BEGIN: transpilers to catch StationStore::remoteOrder changes
            // TODO: IL 1522 there is one with the this pointer and one with SUB (c# 300)
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Add),
                    new CodeMatch(OpCodes.Stfld))
            .Repeat(matcher =>
            {
                // c# 144 IL 686
                if(RemOrderCounter == 0)
                {
                    matcher
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 5))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 4))
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc>((StationComponent stationComponent, ref SupplyDemandPair supplyDemandPair) =>
                        {
                            Debug.Log("TADAAAA 1: " + stationComponent.gid + " " + stationComponent.storage[supplyDemandPair.demandIndex].remoteOrder);
                            return 0;
                        }))
                        .Insert(new CodeInstruction(OpCodes.Pop));
                    RemOrderCounter++;
                }
                // c# 229 IL 1230
                else if(RemOrderCounter == 1)
                {
                    matcher
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 14))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 23))
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc>((StationComponent stationComponent, ref SupplyDemandPair supplyDemandPair) =>
                        {
                            Debug.Log("TADAAAA 2: " + stationComponent.gid + " " + stationComponent.storage[supplyDemandPair.demandIndex].remoteOrder);
                            return 0;
                        }))
                        .Insert(new CodeInstruction(OpCodes.Pop));
                    RemOrderCounter++;
                }
                // c# 861 IL 4242
                else if(RemOrderCounter == 2)
                {
                    matcher
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 135))
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc>((StationComponent stationComponent, ref SupplyDemandPair supplyDemandPair) =>
                        {
                            Debug.Log("TADAAAA 5: " + stationComponent.gid + " " + stationComponent.storage[supplyDemandPair.demandIndex].remoteOrder);
                            return 0;
                        }))
                        .Insert(new CodeInstruction(OpCodes.Pop));
                    RemOrderCounter++;
                }
            }
            )
            .InstructionEnumeration();

            // c# 294 IL 1531
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldarg_S),
                    new CodeMatch(OpCodes.Add),
                    new CodeMatch(OpCodes.Stfld))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 4))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc>((StationComponent stationComponent, ref SupplyDemandPair supplyDemandPair) =>
                {
                    Debug.Log("TADAAAA 8: " + stationComponent.gid + " " + stationComponent.storage[supplyDemandPair.demandIndex].remoteOrder);
                    return 0;
                }))
                .Insert(new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();

            // #c 297 IL 1541
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldarg_S),
                    new CodeMatch(OpCodes.Sub),
                    new CodeMatch(OpCodes.Stfld))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 14))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 4))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc>((StationComponent stationComponent, ref SupplyDemandPair supplyDemandPair) =>
                {
                    Debug.Log("TADAAAA 9: " + stationComponent.gid + " " + stationComponent.storage[supplyDemandPair.demandIndex].remoteOrder);
                    return 0;
                }))
                .Insert(new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();

            // c# 358 IL 1758
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Sub),
                    new CodeMatch(OpCodes.Stfld))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 34))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc2>((StationComponent stationComponent, int index) =>
                {
                    Debug.Log("TADAAAA 3: " + stationComponent.gid + " " + stationComponent.storage[index].remoteOrder);
                    return 0;
                }))
                .Insert(new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();

            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Sub),
                    new CodeMatch(OpCodes.Stfld))
                .Repeat(matcher =>
                {
                    // c# 818 IL 4062 AND c# 877 IL 4309
                    if (RemOrderCounter2 == 0 || RemOrderCounter2 == 1)
                    {
                        matcher
                            .Advance(1)
                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 130), // get stationComponent3
                                        new CodeInstruction(OpCodes.Ldarg_0), // get this ptr
                                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), "workShipOrders")), // get workShipOrders[] from this ptr
                                        new CodeInstruction(OpCodes.Ldloc_S, 34), // get loop iterator j
                                        new CodeInstruction(OpCodes.Ldelema, typeof(RemoteLogisticOrder)), // get RemoteLogisticOrder from this.workShipOrders[j]
                                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(RemoteLogisticOrder), "otherIndex"))) // get this.workShipOrders[j].otherIndex
                            .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc2>((StationComponent stationComponent, int index) =>
                            {
                                Debug.Log("TADAAAA 4/6: " + stationComponent.gid + " " + stationComponent.storage[index].remoteOrder);
                                return 0;
                            }))
                            .Insert(new CodeInstruction(OpCodes.Pop));
                        RemOrderCounter2++;
                    }
                })
                .InstructionEnumeration();

            // c# 886 IL 4368
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Add),
                    new CodeMatch(OpCodes.Stfld))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), "workShipOrders")),
                                    new CodeInstruction(OpCodes.Ldloc_S, 34),
                                    new CodeInstruction(OpCodes.Ldelema, typeof(RemoteLogisticOrder)),
                                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(RemoteLogisticOrder), "thisIndex")))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc2>((StationComponent stationComponent, int index) =>
                {
                    Debug.Log("TADAAAA 7: " + stationComponent.gid + " " + stationComponent.storage[index].remoteOrder);
                    return 0;
                }))
                .Insert(new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();

            return instructions;
        }
    }
}
