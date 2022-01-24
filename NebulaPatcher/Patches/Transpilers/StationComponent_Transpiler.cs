using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

// thanks tanu and Therzok for the tipps!
namespace NebulaPatcher.Patches.Transpilers
{
#pragma warning disable Harmony003 // Harmony non-ref patch parameters modified
    [HarmonyPatch(typeof(StationComponent))]
    public class StationComponent_Transpiler
    {
        /*
        // desc of function to inject into InternalTickRemote after an addItem() call
        private delegate int ShipFunc(StationComponent stationComponent, ref ShipData shipData);

        private delegate int RemOrderFunc(StationComponent stationComponent, ref SupplyDemandPair supplyDemandPair);

        private delegate int RemOrderFunc2(StationComponent stationComponent, int index);

        private delegate int RemOrderFunc3(StationComponent stationComponent, StationComponent[] gStationPool, int n);

        private delegate int CheckgStationPool(ref ShipData shipData);

        private delegate int TakeItem(StationComponent stationComponent, int storageIndex, int amount);

        private delegate int EnergyCost(StationComponent stationComponent, long cost);

        private static int TakeItemCounter = 0;
        private static int RemOrderCounter = 0;
        private static int RemOrderCounter2 = 0;
        private static int RemOrderCounter3 = 0;

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(StationComponent.RematchRemotePairs))]
        public static IEnumerable<CodeInstruction> RematchRemotePairs_Transpiler(IEnumerable<CodeInstruction> instructions)
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
                                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), nameof(StationComponent.workShipOrders))),
                                        new CodeInstruction(OpCodes.Ldloc_S, 10),
                                        new CodeInstruction(OpCodes.Ldelema, typeof(RemoteLogisticOrder)),
                                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(RemoteLogisticOrder), nameof(RemoteLogisticOrder.thisIndex))))
                    .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc2>((StationComponent stationComponent, int index) =>
                    {
                        if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                        {
                            List<NebulaConnection> subscribers = Multiplayer.Session.StationsUI.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
                            ILSRemoteOrderData packet = new ILSRemoteOrderData(stationComponent.gid, index, stationComponent.storage[index].remoteOrder);
                            for (int i = 0; i < subscribers.Count; i++)
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
                        if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                        {
                            int gIndex = stationComponent.workShipDatas[n].otherGId;
                            StationStore[] storeArray = gStationComponent[gIndex]?.storage;
                            if (storeArray != null)
                            {
                                List<NebulaConnection> subscribers = Multiplayer.Session.StationsUI.GetSubscribers(gStationComponent[gIndex].planetId, gStationComponent[gIndex].id, gStationComponent[gIndex].gid);

                                int otherIndex = stationComponent.workShipOrders[n].otherIndex;
                                ILSRemoteOrderData packet = new ILSRemoteOrderData(gStationComponent[gIndex].gid, otherIndex, storeArray[otherIndex].remoteOrder);
                                for (int i = 0; i < subscribers.Count; i++)
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
                if (RemOrderCounter3 == 0)
                {
                    matcher
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                            new CodeInstruction(OpCodes.Ldloc_S, 14))
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc2>((StationComponent stationComponent, int index) =>
                        {
                            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                            {
                                List<NebulaConnection> subscribers = Multiplayer.Session.StationsUI.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
                                ILSRemoteOrderData packet = new ILSRemoteOrderData(stationComponent.gid, index, stationComponent.storage[index].remoteOrder);
                                for (int i = 0; i < subscribers.Count; i++)
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
                            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                            {
                                int gIndex = stationComponent.workShipDatas[n].otherGId;
                                StationStore[] storeArray = gStationComponent[gIndex]?.storage;
                                if (storeArray != null)
                                {
                                    List<NebulaConnection> subscribers = Multiplayer.Session.StationsUI.GetSubscribers(gStationComponent[gIndex].planetId, gStationComponent[gIndex].id, gStationComponent[gIndex].gid);
                                    int otherIndex = stationComponent.workShipOrders[n].otherIndex;
                                    ILSRemoteOrderData packet = new ILSRemoteOrderData(gStationComponent[gIndex].gid, otherIndex, storeArray[otherIndex].remoteOrder);
                                    for (int i = 0; i < subscribers.Count; i++)
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
                else if (RemOrderCounter3 == 1)
                {
                    matcher
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                            new CodeInstruction(OpCodes.Ldloc_S, 18)) // this is the only difference
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc2>((StationComponent stationComponent, int index) =>
                        {
                            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                            {
                                List<NebulaConnection> subscribers = Multiplayer.Session.StationsUI.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
                                ILSRemoteOrderData packet = new ILSRemoteOrderData(stationComponent.gid, index, stationComponent.storage[index].remoteOrder);
                                for (int i = 0; i < subscribers.Count; i++)
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
                            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                            {
                                int gIndex = stationComponent.workShipDatas[n].otherGId;
                                StationStore[] storeArray = gStationComponent[gIndex]?.storage;
                                if (storeArray != null)
                                {
                                    List<NebulaConnection> subscribers = Multiplayer.Session.StationsUI.GetSubscribers(gStationComponent[gIndex].planetId, gStationComponent[gIndex].id, gStationComponent[gIndex].gid);
                                    int otherIndex = stationComponent.workShipOrders[n].otherIndex;
                                    ILSRemoteOrderData packet = new ILSRemoteOrderData(gStationComponent[gIndex].gid, otherIndex, storeArray[otherIndex].remoteOrder);
                                    for (int i = 0; i < subscribers.Count; i++)
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
        [HarmonyPatch(nameof(StationComponent.InternalTickRemote))]
        public static IEnumerable<CodeInstruction> InternalTickRemote_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // get all the AddItem calls
            // c# 470
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldc_R4),
                    new CodeMatch(OpCodes.Stfld),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "AddItem"),
                    new CodeMatch(OpCodes.Pop))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 51))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<ShipFunc>((StationComponent stationComponent, ref ShipData shipData) =>
                {
                    if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        ILSShipItems packet = new ILSShipItems(true, shipData.itemId, shipData.itemCount, shipData.shipIndex, stationComponent.gid, shipData.inc);
                        Multiplayer.Session.Network.SendPacketToStar(packet, GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                    }
                    return 0;
                }))
                .Insert(new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();
            // c# 937
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ble),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "AddItem"),
                    new CodeMatch(OpCodes.Pop))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 138))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 51))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<ShipFunc>((StationComponent stationComponent, ref ShipData shipData) =>
                {
                    if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        ILSShipItems packet = new ILSShipItems(true, shipData.itemId, shipData.itemCount, shipData.shipIndex, stationComponent.gid, shipData.inc);
                        Multiplayer.Session.Network.SendPacketToStar(packet, GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                    }
                    return 0;
                }))
                .Insert(new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();
            // end AddItem calls

            // TakeItem call (c# 1021)
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
                    new CodeMatch(OpCodes.Stfld))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 138))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 51))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<ShipFunc>((StationComponent stationComponent, ref ShipData shipData) =>
                {
                    if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        ILSShipItems packet = new ILSShipItems(false, shipData.itemId, shipData.itemCount, shipData.shipIndex, stationComponent.gid, shipData.inc);
                        Multiplayer.Session.Network.SendPacketToStar(packet, GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                    }
                    return 0;
                }))
                .Insert(new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();

            // inofficial TakeItem calls at c# 219, c# 336
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), nameof(StationComponent.storage))),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(SupplyDemandPair), nameof(SupplyDemandPair.supplyIndex))),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldflda),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldind_I4),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Sub),
                    new CodeMatch(OpCodes.Stind_I4))
                .Repeat(matcher =>
                {
                    if (TakeItemCounter == 0)
                    {
                        matcher
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 27))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SupplyDemandPair), nameof(SupplyDemandPair.supplyIndex))))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 34))
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<TakeItem>((StationComponent stationComponent, int storageIndex, int amount) =>
                        {
                            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                            {
                                // TODO: use correct inc value
                                ILSShipItems packet = new ILSShipItems(false, stationComponent.storage[storageIndex].itemId, amount, 0, stationComponent.gid, 0);
                                Multiplayer.Session.Network.SendPacketToStar(packet, GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                            }
                            return 0;
                        }))
                        .Insert(new CodeInstruction(OpCodes.Pop))
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 42))
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<EnergyCost>((StationComponent stationComponent, long cost) =>
                        {
                            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                            {
                                Multiplayer.Session.Network.SendPacketToStar(new ILSEnergyConsumeNotification(stationComponent.gid, cost), GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                            }
                            return 0;
                        }))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Pop));

                        TakeItemCounter++;
                    }
                    else if (TakeItemCounter == 1)
                    {
                        matcher
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 46))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SupplyDemandPair), nameof(SupplyDemandPair.supplyIndex))))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 47))
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<TakeItem>((StationComponent stationComponent, int storageIndex, int amount) =>
                        {
                            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                            {
                                // TODO: use correct inc value
                                ILSShipItems packet = new ILSShipItems(false, stationComponent.storage[storageIndex].itemId, amount, 0, stationComponent.gid, 0);
                                Multiplayer.Session.Network.SendPacketToStar(packet, GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                            }
                            return 0;
                        }))
                        .Insert(new CodeInstruction(OpCodes.Pop))
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 42))
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<EnergyCost>((StationComponent stationComponent, long cost) =>
                        {
                            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                            {
                                Multiplayer.Session.Network.SendPacketToStar(new ILSEnergyConsumeNotification(stationComponent.gid, cost), GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                            }
                            return 0;
                        }))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Pop));

                        TakeItemCounter++;
                    }
                })
                .InstructionEnumeration();

            // inofficial TakeItem calls at c# 995
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(SupplyDemandPair), nameof(SupplyDemandPair.supplyIndex))),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldflda, AccessTools.Field(typeof(StationStore), nameof(StationStore.count))),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldind_I4),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Sub),
                    new CodeMatch(OpCodes.Stind_I4))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 138))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 142))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SupplyDemandPair), nameof(SupplyDemandPair.supplyIndex))))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 143))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<TakeItem>((StationComponent stationComponent, int storageIndex, int amount) =>
                {
                    if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        // TODO: use correct inc value
                        ILSShipItems packet = new ILSShipItems(false, stationComponent.storage[storageIndex].itemId, amount, 0, stationComponent.gid, 0);
                        Multiplayer.Session.Network.SendPacketToStar(packet, GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                    }
                    return 0;
                }))
                .Insert(new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();

            // remoteOrder changes
            // c# 209, c# 326
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Call),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldflda),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldind_I4),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Add),
                    new CodeMatch(OpCodes.Stind_I4))
                .Repeat(matcher =>
                {
                    if (RemOrderCounter == 0)
                    {
                        matcher
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 28))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 27))
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc>((StationComponent stationComponent, ref SupplyDemandPair supplyDemandPair) =>
                        {
                            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                            {
                                List<NebulaConnection> subscribers = Multiplayer.Session.StationsUI.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
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
                    else if (RemOrderCounter == 1)
                    {
                        matcher
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 38))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 46))
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc>((StationComponent stationComponent, ref SupplyDemandPair supplyDemandPair) =>
                        {
                            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                            {
                                List<NebulaConnection> subscribers = Multiplayer.Session.StationsUI.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
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
                })
                .InstructionEnumeration();
            // c# 408
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Call),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldflda),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldind_I4),
                    new CodeMatch(OpCodes.Ldarg_S),
                    new CodeMatch(OpCodes.Add),
                    new CodeMatch(OpCodes.Stind_I4))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 27))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc>((StationComponent stationComponent, ref SupplyDemandPair supplyDemandPair) =>
                {
                    if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        List<NebulaConnection> subscribers = Multiplayer.Session.StationsUI.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
                        ILSRemoteOrderData packet = new ILSRemoteOrderData(stationComponent.gid, supplyDemandPair.demandIndex, stationComponent.storage[supplyDemandPair.demandIndex].remoteOrder);
                        for (int i = 0; i < subscribers.Count; i++)
                        {
                            subscribers[i].SendPacket(packet);
                        }
                    }
                    return 0;
                }))
                .Insert(new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();
            // c# 415
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Call),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldflda),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldind_I4),
                    new CodeMatch(OpCodes.Ldarg_S),
                    new CodeMatch(OpCodes.Sub),
                    new CodeMatch(OpCodes.Stind_I4))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 38))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 27))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc>((StationComponent stationComponent, ref SupplyDemandPair supplyDemandPair) =>
                {
                    if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        List<NebulaConnection> subscribers = Multiplayer.Session.StationsUI.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
                        ILSRemoteOrderData packet = new ILSRemoteOrderData(stationComponent.gid, supplyDemandPair.demandIndex, stationComponent.storage[supplyDemandPair.demandIndex].remoteOrder);
                        for (int i = 0; i < subscribers.Count; i++)
                        {
                            subscribers[i].SendPacket(packet);
                        }
                    }
                    return 0;
                }))
                .Insert(new CodeInstruction(OpCodes.Pop))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 42))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<EnergyCost>((StationComponent stationComponent, long cost) =>
                {
                    if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        Multiplayer.Session.Network.SendPacketToStar(new ILSEnergyConsumeNotification(stationComponent.gid, cost), GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                    }
                    return 0;
                }))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();
                                  // c# 480, c# 948, c# 1033
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldflda),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldind_I4),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Sub),
                    new CodeMatch(OpCodes.Stind_I4))
                .Repeat(matcher =>
                {
                    if (RemOrderCounter2 == 0)
                    {
                        matcher
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), nameof(StationComponent.workShipOrders))),
                                            new CodeInstruction(OpCodes.Ldloc_S, 50),
                                            new CodeInstruction(OpCodes.Ldelema, typeof(RemoteLogisticOrder)),
                                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(RemoteLogisticOrder), nameof(RemoteLogisticOrder.thisIndex))))
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc2>((StationComponent stationComponent, int index) =>
                        {
                            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                            {
                                if (index > 4)
                                {
                                    // needed as some times game passes 5 as index causing out of bounds exception (really weird this happens..)
                                    return 0;
                                }
                                List<NebulaConnection> subscribers = Multiplayer.Session.StationsUI.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
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
                    else if (RemOrderCounter2 == 1)
                    {
                        matcher
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 138))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), nameof(StationComponent.workShipOrders))),
                                            new CodeInstruction(OpCodes.Ldloc_S, 50),
                                            new CodeInstruction(OpCodes.Ldelema, typeof(RemoteLogisticOrder)),
                                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(RemoteLogisticOrder), nameof(RemoteLogisticOrder.otherIndex))))
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc2>((StationComponent stationComponent, int index) =>
                        {
                            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                            {
                                if (index > 4)
                                {
                                    // needed as some times game passes 5 as index causing out of bounds exception (really weird this happens..)
                                    return 0;
                                }
                                List<NebulaConnection> subscribers = Multiplayer.Session.StationsUI.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
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
                    else if (RemOrderCounter2 == 2)
                    {
                        matcher
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 138))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), nameof(StationComponent.workShipOrders))),
                                            new CodeInstruction(OpCodes.Ldloc_S, 50),
                                            new CodeInstruction(OpCodes.Ldelema, typeof(RemoteLogisticOrder)),
                                            new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(RemoteLogisticOrder), nameof(RemoteLogisticOrder.otherIndex))))
                        .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc2>((StationComponent stationComponent, int index) =>
                        {
                            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                            {
                                if (index > 4)
                                {
                                    // needed as some times game passes 5 as index causing out of bounds exception (really weird this happens..)
                                    return 0;
                                }
                                List<NebulaConnection> subscribers = Multiplayer.Session.StationsUI.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
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
            // c# 1007
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Call),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldflda),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldind_I4),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Add),
                    new CodeMatch(OpCodes.Stind_I4))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 142))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc>((StationComponent stationComponent, ref SupplyDemandPair supplyDemandPair) =>
                {
                    if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        List<NebulaConnection> subscribers = Multiplayer.Session.StationsUI.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
                        ILSRemoteOrderData packet = new ILSRemoteOrderData(stationComponent.gid, supplyDemandPair.demandIndex, stationComponent.storage[supplyDemandPair.demandIndex].remoteOrder);
                        for (int i = 0; i < subscribers.Count; i++)
                        {
                            subscribers[i].SendPacket(packet);
                        }
                    }
                    return 0;
                }))
                .Insert(new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();
            // c# 1046
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
                    new CodeMatch(OpCodes.Ldflda),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldind_I4),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Add),
                    new CodeMatch(OpCodes.Stind_I4))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0),
                                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), nameof(StationComponent.workShipOrders))),
                                    new CodeInstruction(OpCodes.Ldloc_S, 50),
                                    new CodeInstruction(OpCodes.Ldelema, typeof(RemoteLogisticOrder)),
                                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(RemoteLogisticOrder), nameof(RemoteLogisticOrder.thisIndex))))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<RemOrderFunc2>((StationComponent stationComponent, int index) =>
                {
                    if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        if (index > 4)
                        {
                            // needed as some times game passes 5 as index causing out of bounds exception (really weird this happens..)
                            return 0;
                        }
                        List<NebulaConnection> subscribers = Multiplayer.Session.StationsUI.GetSubscribers(stationComponent.planetId, stationComponent.id, stationComponent.gid);
                        ILSRemoteOrderData packet = new ILSRemoteOrderData(stationComponent.gid, index, stationComponent.storage[index].remoteOrder);
                        for (int i = 0; i < subscribers.Count; i++)
                        {
                            subscribers[i].SendPacket(packet);
                        }
                    }
                    return 0;
                }))
                .Insert(new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();
            // end remoteOrder changes

            // shipData.warperCnt changes begin
            // c# 590
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldflda, AccessTools.Field(typeof(ShipData), nameof(ShipData.warperCnt))),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldind_I4),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Sub),
                    new CodeMatch(OpCodes.Stind_I4))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 51))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<ShipFunc>((StationComponent stationComponent, ref ShipData shipData) =>
                {
                    if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        Multiplayer.Session.Network.SendPacket(new ILSShipUpdateWarperCnt(stationComponent.gid, shipData.shipIndex, shipData.warperCnt));
                    }
                    return 0;
                }))
                .Insert(new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();
            // c# 930
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldflda, AccessTools.Field(typeof(ShipData), nameof(ShipData.warperCnt))),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldind_I4),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Add),
                    new CodeMatch(OpCodes.Stind_I4))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 51))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<ShipFunc>((StationComponent stationComponent, ref ShipData shipData) =>
                {
                    if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        Multiplayer.Session.Network.SendPacket(new ILSShipUpdateWarperCnt(stationComponent.gid, shipData.shipIndex, shipData.warperCnt));
                    }
                    return 0;
                }))
                .Insert(new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();
            // shipData.warperCnt changes end

            // StationComponent warperCount changes
            // c# 16
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), nameof(StationComponent.warperCount))),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Add),
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(StationComponent), nameof(StationComponent.warperCount))))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<StationComponent, int>>(stationComponent =>
                {
                    if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        Multiplayer.Session.Network.SendPacket(new StationUI(stationComponent.planetId, stationComponent.id, stationComponent.gid, StationUI.EUISettings.SetWarperCount, stationComponent.warperCount, true));
                    }
                    return 0;
                }))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();
            // c# 931
            instructions = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), nameof(StationComponent.warperCount))),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Sub),
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(StationComponent), nameof(StationComponent.warperCount))))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 138))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<StationComponent, int>>(stationComponent =>
                {
                    if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        Multiplayer.Session.Network.SendPacket(new StationUI(stationComponent.planetId, stationComponent.id, stationComponent.gid, StationUI.EUISettings.SetWarperCount, stationComponent.warperCount));
                    }
                    return 0;
                }))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();
            // StationComponent warperCount changes end

            // energy consumption start
            // c# 221, c# 338, c# 420 (weey)
            //
            // energy consumption transpiler moved up because for whatever reason it did not work as standalone here.
            // energy consumption end

            return instructions;
        }
        */
        
        [HarmonyReversePatch]
        [HarmonyPatch(nameof(StationComponent.InternalTickRemote))]
        public static void ILSUpdateShipPos(StationComponent stationComponent, PlanetFactory factory, int timeGene, double dt, float shipSailSpeed, float shipWarpSpeed, int shipCarries, StationComponent[] gStationPool, AstroPose[] astroPoses, VectorLF3 relativePos, Quaternion relativeRot, bool starmap, int[] consumeRegister)
        {

            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                // find begin of ship movement computation, c# 436 IL 2090
                CodeMatcher matcher = new CodeMatcher(instructions, il);
                int indexStart = matcher
                    .MatchForward(false,
                        new CodeMatch(i => i.IsLdarg()),
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
                for (matcher.Start().Advance(6); matcher.Pos < indexStart;)
                {
                    matcher.SetAndAdvance(OpCodes.Nop, null);
                }

                // add null check at the beginning of the while(){} for gStationPool[shipData.otherGId] and if it is null skip this shipData until all data received from server
                matcher
                    .MatchForward(true,
                        new CodeMatch(OpCodes.Br),
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), "workShipDatas")),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldelem, typeof(ShipData)),
                        new CodeMatch(OpCodes.Stloc_S));
                object jmpNextLoopIter = matcher.InstructionAt(-5).operand;
                matcher.CreateLabelAt(matcher.Pos + 1, out Label jmpNormalFlow);
                matcher
                    .Advance(1)
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 55)) // j
                    .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<int, int>>((int j) =>
                    {
                        Log.Warn($"Working on index: {j}");
                        return 0;
                    }))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Pop))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_S, 7)) // gStationPool
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 56)) // shipData
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "otherGId")))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldelem, typeof(StationComponent)))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue, jmpNormalFlow))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 55)) // j
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Add))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_S, 55))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Br, jmpNextLoopIter));

                // remove c# 498-520 (adding item from landing ship to station and modify remote order and shifitng those arrays)
                indexStart = matcher
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "itemId")),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "itemCount")),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "inc")),
                        new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "AddItem"))
                    .Pos;
                int indexEnd = matcher
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), "workShipDatas")),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldc_I4_1),
                        new CodeMatch(OpCodes.Add))
                    .Pos;
                /*
                 * The following in exchange to the above would exclude
                 *  Array.Copy(this.workShipDatas, j + 1, this.workShipDatas, j, this.workShipDatas.Length - j - 1);
				 *	Array.Copy(this.workShipOrders, j + 1, this.workShipOrders, j, this.workShipOrders.Length - j - 1);
				 *	this.workShipCount--;
				 *	this.idleShipCount++;
				 *	this.WorkShipBackToIdle(shipData.shipIndex);
				 *	Array.Clear(this.workShipDatas, this.workShipCount, this.workShipDatas.Length - this.workShipCount);
				 *	Array.Clear(this.workShipOrders, this.workShipCount, this.workShipOrders.Length - this.workShipCount);
				 *	
                 * but would produce and endless loop inside while(){} because of the missing decrement of workShipCount resulting in a frozen game:
                 * so either do all of this clientside (hmm) or check again once the transpiler above does work again on 0.9.x game version.
                 * 
                int indexEnd = matcher
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Sub),
                        new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "Clear"),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldc_I4_1),
                        new CodeMatch(OpCodes.Sub),
                        new CodeMatch(OpCodes.Stloc_S))
                    .Pos + 2;
                */
                for (matcher.Start().Advance(indexStart); matcher.Pos < indexEnd;)
                {
                    matcher.SetAndAdvance(OpCodes.Nop, null);
                }

                // c# 617 remove the need for warpers inside of ships to enter warp state to avoid syncing issues there.
                // i think this may caused some issues in previous versions where ships where floating very slow through space for no obvious reason.
                matcher
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "warperCnt")),
                        new CodeMatch(OpCodes.Ldc_I4_0),
                        new CodeMatch(OpCodes.Bgt))
                    .SetAndAdvance(OpCodes.Nop, null)
                    .SetAndAdvance(OpCodes.Nop, null)
                    .SetAndAdvance(OpCodes.Nop, null)
                    .SetOpcodeAndAdvance(OpCodes.Br)
                    // and now remove the code that decrements the ship warper counter (just in case, not really needed)
                    .Advance(4)
                    .SetAndAdvance(OpCodes.Nop, null)
                    .SetAndAdvance(OpCodes.Nop, null)
                    .SetAndAdvance(OpCodes.Nop, null)
                    .SetAndAdvance(OpCodes.Nop, null)
                    .SetAndAdvance(OpCodes.Nop, null)
                    .SetAndAdvance(OpCodes.Nop, null)
                    .SetAndAdvance(OpCodes.Nop, null);

                // remove c# 952 - 1048 (adding item from landing ship to station and modify remote order)
                indexStart = matcher
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Ldarg_S),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "otherGId")),
                        new CodeMatch(OpCodes.Ldelem_Ref),
                        new CodeMatch(OpCodes.Stloc_S),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), "storage")),
                        new CodeMatch(OpCodes.Stloc_S),
                        new CodeMatch(OpCodes.Ldarg_S),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "planetA")))
                    .Pos;
                indexEnd = matcher
                    .MatchForward(true,
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), "remotePairCount")),
                        new CodeMatch(OpCodes.Rem),
                        new CodeMatch(OpCodes.Stloc_S),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Bne_Un))
                    .Pos;
                for(matcher.Start().Advance(indexStart); matcher.Pos <= indexEnd;)
                {
                    matcher.SetAndAdvance(OpCodes.Nop, null);
                }

                // remove c# 1052 - 1087 (taking item from station and modify remote order)
                indexStart = matcher
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "itemId")),
                        new CodeMatch(OpCodes.Stloc_S),
                        new CodeMatch(OpCodes.Ldarg_S),
                        new CodeMatch(OpCodes.Stloc_S),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldloca_S),
                        new CodeMatch(OpCodes.Ldloca_S),
                        new CodeMatch(OpCodes.Ldloca_S),
                        new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "TakeItem"))
                    .Pos;
                indexEnd = matcher
                    .MatchForward(true,
                        new CodeMatch(OpCodes.Stind_I4),
                        new CodeMatch(OpCodes.Leave),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Brfalse),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Call),
                        new CodeMatch(OpCodes.Endfinally))
                    .Pos;
                for(matcher.Start().Advance(indexStart); matcher.Pos <= indexEnd;)
                {
                    matcher.SetAndAdvance(OpCodes.Nop, null);
                }

                return matcher.InstructionEnumeration();
            }

            _ = Transpiler(null, null);
        }
    }
#pragma warning restore Harmony003 // Harmony non-ref patch parameters modified
}