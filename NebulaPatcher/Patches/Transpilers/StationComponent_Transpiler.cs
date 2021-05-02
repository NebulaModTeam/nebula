using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using NebulaWorld;
using NebulaWorld.Logistics;
using NebulaModel.Packets.Logistics;
using UnityEngine;
using System;
using NebulaModel.Networking;

// thanks tanu and Therzok for the tipps!
namespace NebulaPatcher.Patches.Transpilers
{
    [HarmonyPatch(typeof(StationComponent))]
    public class StationComponent_Transpiler
    {
        // desc of function to inject into InternalTickRemote after an addItem() call
        delegate int ShipFunc(StationComponent stationComponent, ref ShipData shipData);
        delegate int RemOrderFunc(StationComponent stationComponent, ref SupplyDemandPair supplyDemandPair);
        delegate int RemOrderFunc2(StationComponent stationComponent, int index);
        delegate int RemOrderFunc3(StationComponent stationComponent, StationComponent[] gStationPool, int n);
        delegate int CheckgStationPool(ref ShipData shipData);
        delegate int TakeItem(StationComponent stationComponent, int storageIndex, int amount);
        delegate int EnergyCost(StationComponent stationComponent, long cost);

        private static int RemOrderCounter = 0;
        private static int RemOrderCounter2 = 0;
        private static int RemOrderCounter3 = 0;

        [HarmonyTranspiler]
        [HarmonyPatch("RematchRemotePairs")]
        public static IEnumerable<CodeInstruction> RematchRemotePairs_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            // BEGIN: transpilers to catch StationStore::remoteOrder changes
            // c# 66 IL 371 AND c# 119 IL 621 AND c# 143 IL 754 AND c# 166 IL 897 AND c# 192 IL 1033
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
            .Repeat(matcher =>
            {
                matcher
                    .Advance(1)
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                        new CodeInstruction(OpCodes.Ldarg_0),
                                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), "workShipOrders")),
                                        new CodeInstruction(OpCodes.Ldloc_S, 10),
                                        new CodeInstruction(OpCodes.Ldelema, typeof(RemoteLogisticOrder)),
                                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(RemoteLogisticOrder), "thisIndex")))
                    .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc2>((StationComponent stationComponent, int index) =>
                    {
                        if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                        {
                            List<NebulaConnection> subscribers = StationUIManager.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
                            ILSRemoteOrderData packet = new ILSRemoteOrderData(stationComponent.gid, index, stationComponent.storage[index].remoteOrder);
                            for(int i = 0; i < subscribers.Count; i++)
                            {
                                subscribers[i].SendPacket(packet);
                            }
                        }
                        return 0;
                    }))
                    .Insert(new CodeInstruction(OpCodes.Pop));
            })
            .InstructionEnumeration();

            // c# 72 IL 403 AND c# 125 IL 660 AND c# 172 IL 929 AND c# 198 IL 1065
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
                matcher
                    .Advance(1)
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                        new CodeInstruction(OpCodes.Ldarg_1),
                                        new CodeInstruction(OpCodes.Ldloc_S, 10))
                    .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc3>((StationComponent stationComponent, StationComponent[] gStationComponent, int n) =>
                    {
                        if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                        {
                            int gIndex = stationComponent.workShipDatas[n].otherGId;
                            StationStore[] storeArray = gStationComponent[gIndex]?.storage;
                            if (storeArray != null)
                            {
                                List<NebulaConnection> subscribers = StationUIManager.GetSubscribers(gStationComponent[gIndex].planetId, gStationComponent[gIndex].id, gStationComponent[gIndex].gid);
                                
                                int otherIndex = stationComponent.workShipOrders[n].otherIndex;
                                ILSRemoteOrderData packet = new ILSRemoteOrderData(gStationComponent[gIndex].gid, otherIndex, storeArray[otherIndex].remoteOrder);
                                for(int i = 0; i < subscribers.Count; i++)
                                {
                                    subscribers[i].SendPacket(packet);
                                }
                            }
                        }
                        return 0;
                    }))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Pop));
            })
            .InstructionEnumeration();

            // c# 93 IL 508 AND c# 221 IL 1156
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldarg_S),
                    new CodeMatch(OpCodes.Add),
                    new CodeMatch(OpCodes.Stfld))
            .Repeat(matcher =>
            {
                if(RemOrderCounter3 == 0)
                {
                    matcher
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                            new CodeInstruction(OpCodes.Ldloc_S, 14))
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc2>((StationComponent stationComponent, int index) =>
                        {
                            if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                            {
                                List<NebulaConnection> subscribers = StationUIManager.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
                                ILSRemoteOrderData packet = new ILSRemoteOrderData(stationComponent.gid, index, stationComponent.storage[index].remoteOrder);
                                for(int i = 0; i < subscribers.Count; i++)
                                {
                                    subscribers[i].SendPacket(packet);
                                }
                            }
                            return 0;
                        }))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Pop))
                        .Advance(9) // TODO: check if this should be 9 or 8
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                            new CodeInstruction(OpCodes.Ldarg_1),
                                            new CodeInstruction(OpCodes.Ldloc_S, 10))
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc3>((StationComponent stationComponent, StationComponent[] gStationComponent, int n) =>
                        {
                            if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                            {
                                int gIndex = stationComponent.workShipDatas[n].otherGId;
                                StationStore[] storeArray = gStationComponent[gIndex]?.storage;
                                if (storeArray != null)
                                {
                                    List<NebulaConnection> subscribers = StationUIManager.GetSubscribers(gStationComponent[gIndex].planetId, gStationComponent[gIndex].id, gStationComponent[gIndex].gid);
                                    int otherIndex = stationComponent.workShipOrders[n].otherIndex;
                                    ILSRemoteOrderData packet = new ILSRemoteOrderData(gStationComponent[gIndex].gid, otherIndex, storeArray[otherIndex].remoteOrder);
                                    for(int i = 0; i < subscribers.Count; i++)
                                    {
                                        subscribers[i].SendPacket(packet);
                                    }
                                }
                            }
                            return 0;
                        }))
                        .Insert(new CodeInstruction(OpCodes.Pop));
                    RemOrderCounter3++;
                }
                else if(RemOrderCounter3 == 1)
                {
                    matcher
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                            new CodeInstruction(OpCodes.Ldloc_S, 18)) // this is the only difference
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc2>((StationComponent stationComponent, int index) =>
                        {
                            if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                            {
                                List<NebulaConnection> subscribers = StationUIManager.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
                                ILSRemoteOrderData packet = new ILSRemoteOrderData(stationComponent.gid, index, stationComponent.storage[index].remoteOrder);
                                for(int i = 0; i < subscribers.Count; i++)
                                {
                                    subscribers[i].SendPacket(packet);
                                }
                            }
                            return 0;
                        }))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Pop))
                        .Advance(9) // TODO: check if this should be 9 or 8
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                            new CodeInstruction(OpCodes.Ldarg_1),
                                            new CodeInstruction(OpCodes.Ldloc_S, 10))
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc3>((StationComponent stationComponent, StationComponent[] gStationComponent, int n) =>
                        {
                            if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                            {
                                int gIndex = stationComponent.workShipDatas[n].otherGId;
                                StationStore[] storeArray = gStationComponent[gIndex]?.storage;
                                if (storeArray != null)
                                {
                                    List<NebulaConnection> subscribers = StationUIManager.GetSubscribers(gStationComponent[gIndex].planetId, gStationComponent[gIndex].id, gStationComponent[gIndex].gid);
                                    int otherIndex = stationComponent.workShipOrders[n].otherIndex;
                                    ILSRemoteOrderData packet = new ILSRemoteOrderData(gStationComponent[gIndex].gid, otherIndex, storeArray[otherIndex].remoteOrder);
                                    for(int i = 0; i < subscribers.Count; i++)
                                    {
                                        subscribers[i].SendPacket(packet);
                                    }
                                }
                            }
                            return 0;
                        }))
                        .Insert(new CodeInstruction(OpCodes.Pop));
                    RemOrderCounter3++;
                }
            })
            .InstructionEnumeration();
            // END: transpilers to catch StationStore::remoteOrder changes
            return instructions;
        }

        [HarmonyTranspiler]
        [HarmonyPatch("InternalTickRemote")]
        public static IEnumerable<CodeInstruction> InternalTickRemote_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            // BEGIN: transpilers to catch addItem() and TakeItem() and energy decrease by ship departure
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
                    LocalPlayer.SendPacketToStar(packet, GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
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
                    LocalPlayer.SendPacketToStar(packet, GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
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
                    LocalPlayer.SendPacketToStar(packet, GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                }
                return 0;
            }))
            .Insert(new CodeInstruction(OpCodes.Pop))
            .InstructionEnumeration();

            // catch inofficial TakeItem() at c# 150 AND 235
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "IdleShipGetToWork"),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Sub),
                    new CodeMatch(OpCodes.Stfld))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                new CodeInstruction(OpCodes.Ldloca_S, 4),
                                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SupplyDemandPair), "supplyIndex")),
                                new CodeInstruction(OpCodes.Ldloc_S, 11))
            .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<TakeItem>((StationComponent stationComponent, int storageIndex, int amount) =>
            {
                if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                {
                    ILSShipItems packet = new ILSShipItems(false, stationComponent.storage[storageIndex].itemId, amount, 0, stationComponent.gid);
                    LocalPlayer.SendPacketToStar(packet, GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                }
                return 0;
            }))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Pop))
            .Insert(new CodeInstruction(OpCodes.Ldarg_0), // grab energy cost for ship departure # 151
                        new CodeInstruction(OpCodes.Ldloc_S, 10),
                        HarmonyLib.Transpilers.EmitDelegate<EnergyCost>((StationComponent stationComponent, long cost) =>
                        {
                            if(SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                            {
                                LocalPlayer.SendPacketToStar(new ILSEnergyConsumeNotification(stationComponent.gid, cost), GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                            }
                            return 0;
                        }),
                        new CodeInstruction(OpCodes.Pop))
            // c# 235
            .MatchForward(true,
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "IdleShipGetToWork"),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Sub),
                    new CodeMatch(OpCodes.Stfld))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                new CodeInstruction(OpCodes.Ldloca_S, 23),
                                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SupplyDemandPair), "supplyIndex")),
                                new CodeInstruction(OpCodes.Ldloc_S, 24))
            .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<TakeItem>((StationComponent stationComponent, int storageIndex, int amount) =>
            {
                if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                {
                    ILSShipItems packet = new ILSShipItems(false, stationComponent.storage[storageIndex].itemId, amount, 0, stationComponent.gid);
                    LocalPlayer.SendPacketToStar(packet, GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                }
                return 0;
            }))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Pop))
            .Insert(new CodeInstruction(OpCodes.Ldarg_0), // grab energy cost for ship departure # 236
                        new CodeInstruction(OpCodes.Ldloc_S, 19),
                        HarmonyLib.Transpilers.EmitDelegate<EnergyCost>((StationComponent stationComponent, long cost) =>
                        {
                            if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                            {
                                LocalPlayer.SendPacketToStar(new ILSEnergyConsumeNotification(stationComponent.gid, cost), GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                            }
                            return 0;
                        }),
                        new CodeInstruction(OpCodes.Pop))
            // find line c# 301 and grab energy cost
            .MatchForward(true,
                new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "IdleShipGetToWork"),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Dup),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Sub),
                new CodeMatch(OpCodes.Stfld))
            .Advance(1)
            .Insert(new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldloc_S, 19),
                        HarmonyLib.Transpilers.EmitDelegate<EnergyCost>((StationComponent stationComponent, long cost) =>
                        {
                            if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                            {
                                LocalPlayer.SendPacketToStar(new ILSEnergyConsumeNotification(stationComponent.gid, cost), GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                            }
                            return 0;
                        }),
                        new CodeInstruction(OpCodes.Pop))
            .InstructionEnumeration();
            // END: transpilers to catch addItem() and TakeItem() and energy decrease by ship departure

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
            .Repeat(matcher2 =>
            {
                // c# 144 IL 686
                if(RemOrderCounter == 0)
                {
                    matcher2
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 5))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 4))
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc>((StationComponent stationComponent, ref SupplyDemandPair supplyDemandPair) =>
                        {
                            if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                            {
                                List<NebulaConnection> subscribers = StationUIManager.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
                                ILSRemoteOrderData packet = new ILSRemoteOrderData(stationComponent.gid, supplyDemandPair.demandIndex, stationComponent.storage[supplyDemandPair.demandIndex].remoteOrder);
                                for (int i = 0; i < subscribers.Count; i++)
                                {
                                    subscribers[i].SendPacket(packet);
                                }
                            }
                            return 0;
                        }))
                        .Insert(new CodeInstruction(OpCodes.Pop));
                    RemOrderCounter++;
                }
                // c# 229 IL 1230
                else if(RemOrderCounter == 1)
                {
                    matcher2
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 14))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 23))
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc>((StationComponent stationComponent, ref SupplyDemandPair supplyDemandPair) =>
                        {
                            if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                            {
                                List<NebulaConnection> subscribers = StationUIManager.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
                                ILSRemoteOrderData packet = new ILSRemoteOrderData(stationComponent.gid, supplyDemandPair.demandIndex, stationComponent.storage[supplyDemandPair.demandIndex].remoteOrder);
                                for (int i = 0; i < subscribers.Count; i++)
                                {
                                    subscribers[i].SendPacket(packet);
                                }
                            }
                            return 0;
                        }))
                        .Insert(new CodeInstruction(OpCodes.Pop));
                    RemOrderCounter++;
                }
                // c# 861 IL 4242
                else if(RemOrderCounter == 2)
                {
                    matcher2
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 135))
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc>((StationComponent stationComponent, ref SupplyDemandPair supplyDemandPair) =>
                        {
                            if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                            {
                                List<NebulaConnection> subscribers = StationUIManager.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
                                ILSRemoteOrderData packet = new ILSRemoteOrderData(stationComponent.gid, supplyDemandPair.demandIndex, stationComponent.storage[supplyDemandPair.demandIndex].remoteOrder);
                                for (int i = 0; i < subscribers.Count; i++)
                                {
                                    subscribers[i].SendPacket(packet);
                                }
                            }
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
                    if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                    {
                        List<NebulaConnection> subscribers = StationUIManager.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
                        ILSRemoteOrderData packet = new ILSRemoteOrderData(stationComponent.gid, supplyDemandPair.demandIndex, stationComponent.storage[supplyDemandPair.demandIndex].remoteOrder);
                        for(int i = 0; i < subscribers.Count; i++)
                        {
                            subscribers[i].SendPacket(packet);
                        }
                    }
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
                    if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                    {
                        List<NebulaConnection> subscribers = StationUIManager.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
                        ILSRemoteOrderData packet = new ILSRemoteOrderData(stationComponent.gid, supplyDemandPair.supplyIndex, stationComponent.storage[supplyDemandPair.supplyIndex].remoteOrder);
                        for(int i = 0; i < subscribers.Count; i++)
                        {
                            subscribers[i].SendPacket(packet);
                        }
                    }
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
                    if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                    {
                        if(index > 4)
                        {
                            // needed as some times game passes 5 as index causing out of bounds exception (really weird this happens..)
                            return 0;
                        }
                        List<NebulaConnection> subscribers = StationUIManager.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
                        ILSRemoteOrderData packet = new ILSRemoteOrderData(stationComponent.gid, index, stationComponent.storage[index].remoteOrder);
                        for(int i = 0; i < subscribers.Count; i++)
                        {
                            subscribers[i].SendPacket(packet);
                        }
                    }
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
                .Repeat(matcher3 =>
                {
                    // c# 818 IL 4062 AND c# 877 IL 4309
                    if (RemOrderCounter2 == 0 || RemOrderCounter2 == 1)
                    {
                        matcher3
                            .Advance(1)
                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 130), // get stationComponent3
                                        new CodeInstruction(OpCodes.Ldarg_0), // get this ptr
                                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), "workShipOrders")), // get workShipOrders[] from this ptr
                                        new CodeInstruction(OpCodes.Ldloc_S, 34), // get loop iterator j
                                        new CodeInstruction(OpCodes.Ldelema, typeof(RemoteLogisticOrder)), // get RemoteLogisticOrder from this.workShipOrders[j]
                                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(RemoteLogisticOrder), "otherIndex"))) // get this.workShipOrders[j].otherIndex
                            .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc2>((StationComponent stationComponent, int index) =>
                            {
                                if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                                {
                                    if (index > 4)
                                    {
                                        // needed as some times game passes 5 as index causing out of bounds exception (really weird this happens..)
                                        return 0;
                                    }
                                    List<NebulaConnection> subscribers = StationUIManager.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
                                    ILSRemoteOrderData packet = new ILSRemoteOrderData(stationComponent.gid, index, stationComponent.storage[index].remoteOrder);
                                    for (int i = 0; i < subscribers.Count; i++)
                                    {
                                        subscribers[i].SendPacket(packet);
                                    }
                                }
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
                    if(SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
                    {
                        if (index > 4)
                        {
                            // needed as some times game passes 5 as index causing out of bounds exception (really weird this happens..)
                            return 0;
                        }
                        List<NebulaConnection> subscribers = StationUIManager.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
                        ILSRemoteOrderData packet = new ILSRemoteOrderData(stationComponent.gid, index, stationComponent.storage[index].remoteOrder);
                        for(int i = 0; i < subscribers.Count; i++)
                        {
                            subscribers[i].SendPacket(packet);
                        }
                    }
                    return 0;
                }))
                .Insert(new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();
            // END: transpilers to catch StationStore::remoteOrder changes

            // START: transpilers to catch ShipData.warperCnt++ and stationComponent3.warperCount--;
            // c# 807 IL 4011
            instructions = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "warperCnt")),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Add),
                    new CodeMatch(OpCodes.Stfld),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), "warperCount")),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Sub),
                    new CodeMatch(OpCodes.Stfld))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                    new CodeInstruction(OpCodes.Ldloca_S, 35))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<ShipFunc>((StationComponent stationComponent, ref ShipData shipData) =>
                {
                    LocalPlayer.SendPacket(new StationUI(stationComponent.planetId, stationComponent.id, stationComponent.gid, StationUI.EUISettings.SetWarperCount, stationComponent.warperCount));
                    LocalPlayer.SendPacket(new ILSShipUpdateWarperCnt(stationComponent.gid, shipData.shipIndex, shipData.warperCnt));
                    return 0;
                }))
                .Insert(new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();
            // END: transpilers to catch ShipData.warperCnt++

            return instructions;
        }

        [HarmonyReversePatch]
        [HarmonyPatch("InternalTickRemote")]
        public static void ILSUpdateShipPos(StationComponent stationComponent, int timeGene, double dt, float shipSailSpeed, float shipWarpSpeed, int shipCarries, StationComponent[] gStationPool, AstroPose[] astroPoses, VectorLF3 relativePos, Quaternion relativeRot, bool starmap, int[] consumeRegister)
        {

            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {

                // find begin of ship movement computation, c# 309 IL 1599
                int origShipUpdateCodeBeginPos = new CodeMatcher(instructions)
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Ldarg_3),
                        new CodeMatch(OpCodes.Ldc_R4),
                        new CodeMatch(OpCodes.Div),
                        new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "Sqrt"),
                        new CodeMatch(OpCodes.Stloc_S),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Stloc_S),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldc_R4),
                        new CodeMatch(OpCodes.Ble_Un))
                    .Pos;

                // cut out only that part of original function, but keep the first 5 IL lines (they create the 'bool flag' which is needed)
                CodeMatcher matcher = new CodeMatcher(instructions);
                for(int i = 0; i < matcher.Length; i++)
                {
                    if (matcher.Pos < origShipUpdateCodeBeginPos && matcher.Pos > 5)
                    {
                        matcher.SetAndAdvance(OpCodes.Nop, null);
                    }
                    else
                    {
                        matcher.Advance(1);
                    }
                }
                instructions = matcher.InstructionEnumeration();

                // remove c# 352 - 369 IL 118B - 12DA (which is just after the first addItem() call)
                int origTempBlockIndexStart = new CodeMatcher(instructions)
                    .MatchForward(true,
                        new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "AddItem"),
                        new CodeMatch(OpCodes.Pop))
                    .Pos;
                int origTempBlockIndexEnd = new CodeMatcher(instructions)
                    .MatchForward(true,
                        new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "AddItem"))
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Br))
                    .Pos;

                matcher.Start()
                    .Advance(origTempBlockIndexStart + 1);
                for(; matcher.Pos < origTempBlockIndexEnd/* - 4*/;) // note the -4 index here to still include j-- (only if we decrease workShipCount here too)
                {
                    matcher.SetAndAdvance(OpCodes.Nop, null);
                }
                instructions = matcher.InstructionEnumeration();

                // remove c# 814 - 862 IL 4039 - 4248 (TODO: and fetch data from server)
                origTempBlockIndexStart = new CodeMatcher(instructions)
                    .MatchForward(true,
                        new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "AddItem"),
                        new CodeMatch(OpCodes.Pop),
                        new CodeMatch(OpCodes.Ldloca_S),
                        new CodeMatch(OpCodes.Ldc_I4_0),
                        new CodeMatch(OpCodes.Stfld),
                        new CodeMatch(OpCodes.Ldarg_0))
                    .Pos;
                origTempBlockIndexEnd = new CodeMatcher(instructions)
                    .Advance(origTempBlockIndexStart)
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Br))
                    .Pos;

                matcher.Start()
                    .Advance(origTempBlockIndexStart);
                for(; matcher.Pos < origTempBlockIndexEnd;)
                {
                    matcher.SetAndAdvance(OpCodes.Nop, null);
                }
                // now remove the rest of the for loop as it is after the OpCodes.Br for some reason
                // c# 837 - 842 IL 4250 - 4261
                matcher.Advance(1);
                for(int i = 0; i < 12; i++)
                {
                    matcher.SetAndAdvance(OpCodes.Nop, null);
                }
                instructions = matcher.InstructionEnumeration();

                // remove addItem() calls
                instructions = new CodeMatcher(instructions)
                    .MatchForward(false,
                        new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "AddItem"))
                    .SetAndAdvance(OpCodes.Pop, null)
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Pop))
                    .MatchForward(false,
                        new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "AddItem"))
                    .SetAndAdvance(OpCodes.Pop, null)
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Pop))
                    .InstructionEnumeration();

                // remove c# 865 - 891 IL 4266 - 4377 (TODO: and fetch data from server) (NOTE: this does also remove the TakeItem() call)
                origTempBlockIndexStart = new CodeMatcher(instructions)
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Ldloca_S),
                        new CodeMatch(OpCodes.Ldfld),
                        new CodeMatch(OpCodes.Stloc_S),
                        new CodeMatch(OpCodes.Ldarg_S),
                        new CodeMatch(OpCodes.Stloc_S),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldloca_S),
                        new CodeMatch(OpCodes.Ldloca_S),
                        new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "TakeItem"))
                    .Pos;
                origTempBlockIndexEnd = new CodeMatcher(instructions)
                    .Advance(origTempBlockIndexStart)
                    .MatchForward(true,
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldelema),
                        new CodeMatch(OpCodes.Dup),
                        new CodeMatch(OpCodes.Ldfld),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Add),
                        new CodeMatch(OpCodes.Stfld))
                    .Advance(1)
                    .Pos;

                matcher = new CodeMatcher(instructions)
                    .Advance(origTempBlockIndexStart);
                for(; matcher.Pos < origTempBlockIndexEnd;)
                {
                    matcher.SetAndAdvance(OpCodes.Nop, null);
                }
                instructions = matcher.InstructionEnumeration();

                // insert patch to avoid NRE mentioned in issue 59 (gStationPool[shipData.otherGId] == null results in NRE)
                // in case we exit out we still need to call ShipRenderersOnTick() as the ships of this StationComponent would be invisible
                Label jmpLabelDelegate;
                CodeMatcher matcher3 = new CodeMatcher(instructions, il)
                    .MatchForward(true,
                        new CodeMatch(OpCodes.Ldarg_S),
                        new CodeMatch(OpCodes.Ldloca_S),
                        new CodeMatch(OpCodes.Ldfld),
                        new CodeMatch(OpCodes.Ldelema),
                        new CodeMatch(OpCodes.Ldobj),
                        new CodeMatch(OpCodes.Stloc_S),
                        new CodeMatch(OpCodes.Ldloca_S),
                        new CodeMatch(OpCodes.Ldfld),
                        new CodeMatch(OpCodes.Ldc_I4_0),
                        new CodeMatch(OpCodes.Ble))
                .Advance(1)
                // load the StationComponent out of gStationPool[]
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_S, 6), // load gStationPool[]
                                    new CodeInstruction(OpCodes.Ldloca_S, 35), // load shipData
                                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "otherGId")), // load shipData.otherGId
                                    new CodeInstruction(OpCodes.Ldelem_Ref)); // load gStationPool[shipData.otherGId]
                matcher3.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0), // insert call to ShipRenderersOnTick() to not hide any ships :)
                                            new CodeInstruction(OpCodes.Ldarg_S, 7),
                                            new CodeInstruction(OpCodes.Ldarg_S, 8),
                                            new CodeInstruction(OpCodes.Ldarg_S, 9),
                                            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StationComponent), "ShipRenderersOnTick", new Type[] { typeof(AstroPose[]), typeof(VectorLF3), typeof(Quaternion) })),
                                            new CodeInstruction(OpCodes.Ret)); // exit out of original function
                matcher3.CreateLabelAt(matcher3.Pos, out jmpLabelDelegate); // create a label pointing behind the injected code
                matcher3.Advance(-6); // go back to insert jmp
                matcher3.InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue, jmpLabelDelegate)); // if object is NOT null jump to the vanilla code
                instructions = matcher3.InstructionEnumeration();

                // remmove c# 807 and 808 (adding warper from station to ship) IL 4011-4022
                matcher3.Start();
                matcher3.MatchForward(false,
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "warperCnt")),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Add),
                    new CodeMatch(OpCodes.Stfld),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), "warperCount")),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Sub),
                    new CodeMatch(OpCodes.Stfld));
                for(int i = 0; i < 12; i++)
                {
                    matcher3.SetAndAdvance(OpCodes.Nop, null);
                }
                instructions = matcher3.InstructionEnumeration();

                // insert debugging delegate (at the start of the while loop)
                instructions = new CodeMatcher(instructions)
                    .MatchForward(true,
                        new CodeMatch(OpCodes.Stloc_S),
                        new CodeMatch(OpCodes.Br),
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldelema),
                        new CodeMatch(OpCodes.Ldobj))
                    .Advance(1)
                    //.InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 34)) //load j
                    .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<ShipData, ShipData>>(shipData =>
                    {
                        //Debug.Log(shipData.stage + " " + shipData.direction + " " + shipData.t + " " + shipData.uVel + " " + shipData.uPos);
                        return shipData;
                    }))
                    //.InsertAndAdvance(new CodeInstruction(OpCodes.Pop))
                    .InstructionEnumeration();

                return instructions;
            }

            _ = Transpiler(null, null);
        }
    }
}
