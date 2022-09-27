using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
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
        delegate void ShipEnterWarpState(StationComponent stationComponent, int j);
        delegate void AddItem(StationComponent stationComponent, ShipData shipData);
        delegate void TakeItem(StationComponent stationComponent, int itemId, int itemCount, int j);
        delegate void UpdateStorage(StationComponent stationComponent, int index);
        delegate void WorkShipBackToIdle(StationComponent stationComponent, int j, ShipData shipData);

        delegate void RematchRemotePairs(StationComponent stationComponent, int index);
        delegate void SendRematchPacket(StationComponent stationComponent);

        private static int UpdateStorageMatchCounter = 0;

        // theese are needed to craft the ILSRematchRemotePairs packet
        private static List<int> ShipIndex = new List<int>();
        private static List<int> OtherGId = new List<int>();
        private static List<int> ItemId = new List<int>();
        private static List<int> Direction = new List<int>();

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(StationComponent.RematchRemotePairs))]
        public static IEnumerable<CodeInstruction> RematchRemotePairs_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // tell clients about all changes to workShipDatas
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), "workShipDatas")),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(ShipData), "direction")))
                .Repeat(localMatcher =>
                {
                    localMatcher
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 10))
                        .Insert(HarmonyLib.Transpilers.EmitDelegate<RematchRemotePairs>((StationComponent stationComponent, int index) =>
                        {
                            ShipIndex.Add(index);
                            OtherGId.Add(stationComponent.workShipDatas[index].otherGId);
                            ItemId.Add(stationComponent.workShipDatas[index].itemId);
                            Direction.Add(stationComponent.workShipDatas[index].direction);
                        }));
                });

            matcher
                .Start()
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), "workShipDatas")),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldc_I4_M1),
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(ShipData), "direction")))
                .Repeat(localMatcher =>
                {
                    localMatcher
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 10))
                        .Insert(HarmonyLib.Transpilers.EmitDelegate<RematchRemotePairs>((StationComponent stationComponent, int index) =>
                        {
                            ShipIndex.Add(index);
                            OtherGId.Add(stationComponent.workShipDatas[index].otherGId);
                            ItemId.Add(stationComponent.workShipDatas[index].itemId);
                            Direction.Add(stationComponent.workShipDatas[index].direction);
                        }));
                });

            matcher.Start()
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ret))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .Insert(HarmonyLib.Transpilers.EmitDelegate<SendRematchPacket>((StationComponent stationComponent) =>
                {
                    if(Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost && ShipIndex.Count > 0)
                    {
                        Multiplayer.Session.Network.SendPacket(new ILSRematchRemotePairs(stationComponent.gid, ShipIndex, OtherGId, Direction, ItemId));
                    }
                    ShipIndex.Clear();
                    OtherGId.Clear();
                    Direction.Clear();
                    ItemId.Clear();
                }));

            return matcher.InstructionEnumeration();
        }

        // TODO: Update for 0.9.27
        //[HarmonyTranspiler]
        //[HarmonyPatch(nameof(StationComponent.InternalTickRemote))]
        public static IEnumerable<CodeInstruction> InternalTickRemote_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            // tell client when a ship enters warp state. for some reason client gets messed up with warp counter on ships.
            CodeMatcher matcher = new CodeMatcher(instructions, il)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldflda, AccessTools.Field(typeof(ShipData), "warperCnt")),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldind_I4),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Sub),
                    new CodeMatch(OpCodes.Stind_I4),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldflda, AccessTools.Field(typeof(ShipData), "warpState")))
                .Advance(-1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 59))
                .Insert(HarmonyLib.Transpilers.EmitDelegate<ShipEnterWarpState> ((StationComponent stationComponent, int j) =>
                {
                    if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        Multiplayer.Session.Network.SendPacket(new ILSShipEnterWarp(stationComponent.gid, j));
                    }
                }));

            // tell client about WorkShipBackToIdle here as we need to tell him j too
            // c# 522
            matcher.Start()
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "shipIndex")),
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "WorkShipBackToIdle"))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 59))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 60))
                .Insert(HarmonyLib.Transpilers.EmitDelegate<WorkShipBackToIdle>((StationComponent stationComponent, int j, ShipData shipData) =>
                {
                    if(Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        ILSWorkShipBackToIdle packet = new ILSWorkShipBackToIdle(stationComponent, shipData, j);
                        Multiplayer.Session.Network.SendPacket(packet);
                    }
                }));

            // tell clients about AddItem() calls on host
            // c# 502
            matcher.Start()
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "itemId")),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "itemCount")),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "inc")),
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "AddItem"))
                .Advance(2) // also skip Pop call
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 60))
                .Insert(HarmonyLib.Transpilers.EmitDelegate<AddItem>((StationComponent stationComponent, ShipData shipData) =>
                {
                    if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        Multiplayer.Session.Network.SendPacketToStar(new ILSShipAddTake(true, shipData.itemId, shipData.itemCount, stationComponent.gid, shipData.inc), GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                    }
                }));

            // c# 970
            matcher
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "itemId")),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "itemCount")),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "inc")),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "AddItem"))
                .Advance(2) // also skip Pop call
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 147))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 60))
                .Insert(HarmonyLib.Transpilers.EmitDelegate<AddItem>((StationComponent stationComponent, ShipData shipData) =>
                {
                    if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        Multiplayer.Session.Network.SendPacketToStar(new ILSShipAddTake(true, shipData.itemId, shipData.itemCount, stationComponent.gid, shipData.inc), GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                    }
                }));

            // tell clients about TakeItem() calls on host
            matcher
                .MatchForward(true,
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "TakeItem"))
                // just before the call to TakeItem()
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 147))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 60))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "itemId")))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 157))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 59))
                .Insert(HarmonyLib.Transpilers.EmitDelegate<TakeItem>((StationComponent stationComponent, int itemId, int itemCount, int j) =>
                {
                    if(Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        Multiplayer.Session.Network.SendPacketToStar(new ILSShipAddTake(false, itemId, itemCount, stationComponent.gid, j), GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                    }
                }));

            // tell client about StationSTorage[] updates
            // c# 17
            matcher.Start()
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), "storage")),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldflda, AccessTools.Field(typeof(StationStore), "inc")),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldind_I4),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Sub),
                    new CodeMatch(OpCodes.Stind_I4))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 26))
                .Insert(HarmonyLib.Transpilers.EmitDelegate<UpdateStorage>((StationComponent stationComponent, int index) =>
                {
                    if(Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        Multiplayer.Session.Network.SendPacketToStar(new ILSUpdateStorage(stationComponent.gid, index, stationComponent.storage[index].count, stationComponent.storage[index].inc), GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                    }
                }));

            // c# 242, 367
            matcher
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), "storage")),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(SupplyDemandPair), "supplyIndex")),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldflda, AccessTools.Field(typeof(StationStore), "inc")),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldind_I4),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Sub),
                    new CodeMatch(OpCodes.Stind_I4))
                .Repeat(localMatcher =>
                {
                    localMatcher
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, UpdateStorageMatchCounter == 0 ? 30 : 52))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SupplyDemandPair), "supplyIndex")))
                        .Insert(HarmonyLib.Transpilers.EmitDelegate<UpdateStorage>((StationComponent stationComponent, int index) =>
                        {
                            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                            {
                                Multiplayer.Session.Network.SendPacketToStar(new ILSUpdateStorage(stationComponent.gid, index, stationComponent.storage[index].count, stationComponent.storage[index].inc), GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                            }
                        }));

                    UpdateStorageMatchCounter++;
                });
            UpdateStorageMatchCounter = 0; // resetting here as it seems our patches are done twice

            // c# 1034
            matcher.Start()
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(SupplyDemandPair), "supplyIndex")),
                    new CodeMatch(OpCodes.Ldelema),
                    new CodeMatch(OpCodes.Ldflda, AccessTools.Field(typeof(StationStore), "inc")),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Ldind_I4),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Sub),
                    new CodeMatch(OpCodes.Stind_I4))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 147))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 151))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SupplyDemandPair), "supplyIndex")))
                .Insert(HarmonyLib.Transpilers.EmitDelegate<UpdateStorage>((StationComponent stationComponent, int index) =>
                {
                    if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        Multiplayer.Session.Network.SendPacketToStar(new ILSUpdateStorage(stationComponent.gid, index, stationComponent.storage[index].count, stationComponent.storage[index].inc), GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                    }
                }));

            return matcher.InstructionEnumeration();
        }
        
        // TODO: Update for 0.9.27
        //[HarmonyReversePatch]
        //[HarmonyPatch(nameof(StationComponent.InternalTickRemote))]
        public static void ILSUpdateShipPos(StationComponent stationComponent, PlanetFactory factory, int timeGene, double dt, float shipSailSpeed, float shipWarpSpeed, int shipCarries, StationComponent[] gStationPool, AstroData[] astroPoses, VectorLF3 relativePos, Quaternion relativeRot, bool starmap, int[] consumeRegister)
        {

            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                // find begin of ship movement computation, c# 460 IL 2090
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
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_S, 7)) // gStationPool
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 60)) // shipData
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "otherGId")))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldelem, typeof(StationComponent)))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue, jmpNormalFlow))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, 59)) // j
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Add))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_S, 59))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Br, jmpNextLoopIter));
                
                // remove c# 502-525 (adding item from landing ship to station and modify remote order and shifitng those arrays AND j-- (as we end up in an endless loop if not))
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
                    .MatchForward(true,
                        new CodeMatch(OpCodes.Sub),
                        new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "Clear"),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldc_I4_1),
                        new CodeMatch(OpCodes.Sub),
                        new CodeMatch(OpCodes.Stloc_S))
                    .Advance(1)
                    .Pos;
                for (matcher.Start().Advance(indexStart); matcher.Pos < indexEnd;)
                {
                    matcher.SetAndAdvance(OpCodes.Nop, null);
                }

                // c# 621 remove warp state entering as we do this triggered by host. client always messed up here for whatever reason so just tell him what to do.
                matcher
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Ldloca_S),
                        new CodeMatch(OpCodes.Ldflda, AccessTools.Field(typeof(ShipData), "warperCnt")),
                        new CodeMatch(OpCodes.Dup),
                        new CodeMatch(OpCodes.Ldind_I4),
                        new CodeMatch(OpCodes.Ldc_I4_1),
                        new CodeMatch(OpCodes.Sub),
                        new CodeMatch(OpCodes.Stind_I4),
                        new CodeMatch(OpCodes.Ldloca_S),
                        new CodeMatch(OpCodes.Ldflda, AccessTools.Field(typeof(ShipData), "warpState")));
                for(int i = 0; i < 15; i++)
                {
                    matcher.SetAndAdvance(OpCodes.Nop, null);
                }

                // remove c# 956 - 1054 (adding item from landing ship to station and modify remote order)
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
                
                // remove c# 1058 - 1093 (taking item from station and modify remote order)
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