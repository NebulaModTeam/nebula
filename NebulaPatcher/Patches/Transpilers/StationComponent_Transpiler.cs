#region

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using UnityEngine;

#endregion

// thanks tanu and Therzok for the tipps!
namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(StationComponent))]
public class StationComponent_Transpiler
{
    // theese are needed to craft the ILSRematchRemotePairs packet
    private static readonly List<int> ShipIndex = new();
    private static readonly List<int> OtherGId = new();
    private static readonly List<int> ItemId = new();
    private static readonly List<int> Direction = new();

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(StationComponent.RematchRemotePairs))]
    public static IEnumerable<CodeInstruction> RematchRemotePairs_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // tell clients about all changes to workShipDatas
        var matcher = new CodeMatcher(instructions)
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
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, localMatcher.InstructionAt(-5).operand))
                    .Insert(HarmonyLib.Transpilers.EmitDelegate<RematchRemotePairs>((stationComponent, index) =>
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
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, localMatcher.InstructionAt(-5).operand))
                    .Insert(HarmonyLib.Transpilers.EmitDelegate<RematchRemotePairs>((stationComponent, index) =>
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
            .Insert(HarmonyLib.Transpilers.EmitDelegate<SendRematchPacket>(stationComponent =>
            {
                if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost && ShipIndex.Count > 0)
                {
                    Multiplayer.Session.Network.SendPacket(new ILSRematchRemotePairs(stationComponent.gid, ShipIndex, OtherGId,
                        Direction, ItemId));
                }
                ShipIndex.Clear();
                OtherGId.Clear();
                Direction.Clear();
                ItemId.Clear();
            }));

        return matcher.InstructionEnumeration();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(StationComponent.InternalTickRemote))]
    public static IEnumerable<CodeInstruction> InternalTickRemote_Transpiler(IEnumerable<CodeInstruction> instructions,
        ILGenerator il)
    {
        var matcher = new CodeMatcher(instructions, il);

        // Capture the workship index variable (j) in the big while loop
        // c# 500:
        //   while (j < this.workShipCount)
        matcher.End()
            .MatchBack(false,
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "workShipCount"),
                new CodeMatch(OpCodes.Blt));
        var loadWorkShipId =
            new CodeInstruction(matcher.Instruction.opcode, matcher.Instruction.operand); // j => (OpCodes.Ldloc_S, 66)

        #region ILSShipAddTake

        // tell clients about AddItem() calls on host            
        // c# 537:
        //   ...
        //   this.AddItem(ptr3.itemId, ptr3.itemCount, ptr3.inc);
        // >>  Insert (this, ptr3) => ILSShipAddTake packet
        //   factory.NotifyShipDelivery(ptr3.planetB, gStationPool[ptr3.otherGId], ptr3.planetA, this, ptr3.itemId, ptr3.itemCount);
        //   ...
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
            .InsertAndAdvance(matcher.InstructionAt(-9)) // this
            .InsertAndAdvance(matcher.InstructionAt(-9)) // ref ShipData ptr3
            .Insert(HarmonyLib.Transpilers.EmitDelegate<AddItem>((StationComponent stationComponent, ref ShipData shipData) =>
            {
                if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                {
                    Multiplayer.Session.Network.SendPacketToStar(
                        new ILSShipAddTake(true, shipData.itemId, shipData.itemCount, stationComponent.gid, shipData.inc),
                        GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                }
            }));
        var backToStationPos = matcher.Pos;

        // c# 1117:
        //   ...
        //   stationComponent3.AddItem(ptr3.itemId, ptr3.itemCount, ptr3.inc);
        // >>  Insert (stationComponent3, ptr3) => ILSShipAddTake packet
        //   factory.NotifyShipDelivery(ptr3.planetA, this, ptr3.planetB, stationComponent3, ptr3.itemId, ptr3.itemCount);
        //   ...
        matcher
            .MatchForward(true,
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "itemId")),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "itemCount")),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "inc")),
                new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "AddItem"));

        // Capture local variables to use later
        var loadOtherStation = new CodeInstruction(OpCodes.Ldloc_S, matcher.InstructionAt(-7).operand);
        var loadCurrentShip = new CodeInstruction(OpCodes.Ldloc_S, matcher.InstructionAt(-6).operand);

        matcher
            .Advance(2) // also skip Pop call
            .InsertAndAdvance(matcher.InstructionAt(-9)) // stationComponent3
            .InsertAndAdvance(matcher.InstructionAt(-9)) // ref ShipData ptr3
            .Insert(HarmonyLib.Transpilers.EmitDelegate<AddItem>((StationComponent stationComponent, ref ShipData shipData) =>
            {
                if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                {
                    Multiplayer.Session.Network.SendPacketToStar(
                        new ILSShipAddTake(true, shipData.itemId, shipData.itemCount, stationComponent.gid, shipData.inc),
                        GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                }
            }));

        // tell clients about TakeItem() calls on host
        // c# 1208:
        //   ...
        //   int inc;
        // >>  Insert (stationComponent3, itemId3, num120, j) => ILSShipAddTake packet
        //   stationComponent3.TakeItem(ref itemId3, ref num120, out inc);
        //  ...
        matcher
            .MatchForward(true,
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldloca_S),
                new CodeMatch(OpCodes.Ldloca_S),
                new CodeMatch(OpCodes.Ldloca_S),
                new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "TakeItem"))
            // just before the call to TakeItem()
            .InsertAndAdvance(matcher.InstructionAt(-4))
            .InsertAndAdvance(matcher.InstructionAt(-4))
            .InsertAndAdvance(matcher.InstructionAt(-4))
            .InsertAndAdvance(loadWorkShipId)
            .Insert(HarmonyLib.Transpilers.EmitDelegate<TakeItem>(
                (StationComponent stationComponent, ref int itemId, ref int itemCount, int j) =>
                {
                    if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        Multiplayer.Session.Network.SendPacketToStar(
                            new ILSShipAddTake(false, itemId, itemCount, stationComponent.gid, j),
                            GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                    }
                }));

        #endregion

        #region ILSWorkShipBackToIdle

        // tell client about WorkShipBackToIdle here as we need to tell him j too. 
        // because ptr3 is reference, we need to call it before the values get overwritten
        // c# 537:            
        //   this.AddItem(ptr3.itemId, ptr3.itemCount, ptr3.inc);
        // >>  Insert (this, j, ptr3) => ILSWorkShipBackToIdle packet
        //   ...
        //   this.WorkShipBackToIdle(shipIndex);
        matcher.Start()
            .Advance(backToStationPos + 1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(loadWorkShipId)
            .InsertAndAdvance(loadCurrentShip)
            .Insert(HarmonyLib.Transpilers.EmitDelegate<WorkShipBackToIdle>(
                (StationComponent stationComponent, int j, ref ShipData shipData) =>
                {
                    if (!Multiplayer.IsActive || !Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        return;
                    }
                    var packet = new ILSWorkShipBackToIdle(stationComponent, shipData, j);
                    Multiplayer.Session.Network.SendPacket(packet);
                }));

        #endregion

        #region ILSUpdateStorage

        //  tell clients about StationStorage[] updates
        //  c# 253, 378, 1181  IL# 688, 1365, 5032
        //  lock (obj)
        //  {
        //      ...
        //      StationStore[] array = this.storage; (or array = stationComponent3.storage;)
        //      int supplyIndex = supplyDemandPair.supplyIndex; (or supplyIndex = ptr.supplyIndex;)
        //      array[supplyIndex].inc = array[supplyIndex].inc - num;
        //    >>  Insert (stationComponent, supplyIndex) => ILSUpdateStorage packet
        //  }
        matcher.Start()
            .MatchForward(true,
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Ldelema),
                new CodeMatch(OpCodes.Ldflda, AccessTools.Field(typeof(StationStore), "inc")),
                new CodeMatch(OpCodes.Dup),
                new CodeMatch(OpCodes.Ldind_I4),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Sub),
                new CodeMatch(OpCodes.Stind_I4))
            .Repeat(localMatcher =>
            {
                var loadStation = localMatcher.InstructionAt(-10).opcode == OpCodes.Ldarg_0
                    ? new CodeInstruction(OpCodes.Ldarg_0)
                    : //this
                    loadOtherStation; //stationComponent3

                localMatcher
                    .Advance(1)
                    .InsertAndAdvance(loadStation)
                    .InsertAndAdvance(localMatcher.InstructionAt(-10))
                    .InsertAndAdvance(localMatcher.InstructionAt(-10))
                    .Insert(HarmonyLib.Transpilers.EmitDelegate<UpdateStorage>((stationComponent, index) =>
                    {
                        if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
                        {
                            Multiplayer.Session.Network.SendPacketToStar(
                                new ILSUpdateStorage(stationComponent.gid, index, stationComponent.storage[index].count,
                                    stationComponent.storage[index].inc),
                                GameMain.galaxy.PlanetById(stationComponent.planetId).star.id);
                        }
                    }));
            });

        #endregion

        return matcher.InstructionEnumeration();
    }

    [HarmonyReversePatch]
    [HarmonyPatch(nameof(StationComponent.InternalTickRemote))]
    [SuppressMessage("Style", "IDE0060:Remove unused parameter")]
    public static void ILSUpdateShipPos(StationComponent stationComponent, PlanetFactory factory, int timeGene,
        float shipSailSpeed, float shipWarpSpeed, int shipCarries, StationComponent[] gStationPool, AstroData[] astroPoses,
        ref VectorLF3 relativePos, ref Quaternion relativeRot, bool starmap, int[] consumeRegister)
    {
        _ = Transpiler(null, null);
        return;

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            // find begin of ship movement computation, remove all content in if (timeGene == this.gene) {...}
            // remove c# 10 - 473
            var matcher = new CodeMatcher(instructions, il);
            var indexStart = matcher
                .MatchForward(false,
                    new CodeMatch(i => i.IsLdarg()), // float num47 = shipSailSpeed / 600f;
                    new CodeMatch(OpCodes.Ldc_R4),
                    new CodeMatch(OpCodes.Div),
                    new CodeMatch(OpCodes.Stloc_S),
                    new CodeMatch(OpCodes.Ldloc_S), // float num48 = Mathf.Pow(num47, 0.4f);
                    new CodeMatch(OpCodes.Ldc_R4),
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Mathf), "Pow")))
                .Pos;
            // cut out only that part of original function, but keep the first 5 IL lines (they create the 'bool flag' which is needed)
            for (matcher.Start().Advance(6); matcher.Pos < indexStart;)
            {
                matcher.SetAndAdvance(OpCodes.Nop, null);
            }

            // c# 502: add null check at the beginning of the while(){} for gStationPool[shipData.otherGId] and if it is null skip this shipData until all data received from server
            // 
            // 	while (j < this.workShipCount)
            //  {				
            //	   ref ShipData ptr3 = ref this.workShipDatas[j];
            //   >>  insert if (gStationPool[shipData.otherGId] == null) { j++; continue; }
            matcher
                .MatchForward(true,
                    new CodeMatch(OpCodes.Br),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), "workShipDatas")),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldelema, typeof(ShipData)),
                    new CodeMatch(OpCodes.Stloc_S));

            var shipDataRef = matcher.Instruction.operand;
            var loopIndex = matcher.InstructionAt(-2).operand;
            var jmpNextLoopIter = matcher.InstructionAt(-5).operand;
            matcher.CreateLabelAt(matcher.Pos + 1, out var jmpNormalFlow);
            matcher
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_S, 6)) // gStationPool
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, shipDataRef)) // shipData
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "otherGId")))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldelem, typeof(StationComponent)))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue, jmpNormalFlow))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, loopIndex)) // j
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Add))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_S, loopIndex))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Br, jmpNextLoopIter));

            // remove c# 537-561 (adding item from landing ship to station and modify remote order and shifitng those arrays AND j-- (as we end up in an endless loop if not))
            // start: this.AddItem(ptr3.itemId, ptr3.itemCount, ptr3.inc);
            // end:   j--;
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
            var indexEnd = matcher
                .MatchForward(true,
                    new CodeMatch(OpCodes.Sub),
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "Clear"),
                    new CodeMatch(OpCodes.Ldloc_S), // j--
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Sub),
                    new CodeMatch(OpCodes.Stloc_S))
                .Advance(1)
                .Pos;
            for (matcher.Start().Advance(indexStart); matcher.Pos < indexEnd;)
            {
                matcher.SetAndAdvance(OpCodes.Nop, null);
            }

            // remove c# 1103 - 1201 (adding item from landing ship to station and modify remote order)
            // start: StationComponent stationComponent3 = gStationPool[ptr3.otherGId]; StationStore[] array21 = stationComponent3.storage;
            // end:   if (this.remotePairCount > 0) {...}
            indexStart = matcher
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_S),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "otherGId")),
                    new CodeMatch(OpCodes.Ldelem_Ref),
                    new CodeMatch(OpCodes.Stloc_S),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StationComponent), "storage")))
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
            for (matcher.Start().Advance(indexStart); matcher.Pos <= indexEnd;)
            {
                matcher.SetAndAdvance(OpCodes.Nop, null);
            }

            // remove c# 1208 - 1093 (taking item from station and modify remote order)
            // start: stationComponent3.TakeItem(ref itemId3, ref num120, out inc);
            // end:   lock (obj) {...}				
            indexStart = matcher
                .MatchForward(false,
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
            for (matcher.Start().Advance(indexStart); matcher.Pos <= indexEnd;)
            {
                matcher.SetAndAdvance(OpCodes.Nop, null);
            }

            // Switch itemCount in ShipData when ship arrive destination to display correct color
            // Currently the render only test if itemCount > 0 so we can give it a dummy positive value
            // c# 1241:
            //   ptr3.direction = -1;
            // >>  Insert ptr3.itemCount = ptr3.itemCount > 0 ? 0 : 1;
            matcher
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldc_I4_M1),
                    new CodeMatch(OpCodes.Stfld))
                .Advance(1)
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc_S, matcher.InstructionAt(-3).operand),
                    new CodeInstruction(OpCodes.Ldloc_S, matcher.InstructionAt(-3).operand),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ShipData), "itemCount")),
                    new CodeInstruction(OpCodes.Ldc_I4_0),
                    new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(ShipData), "itemCount"))
                )
                .Advance(3) //OpCodes.Ldc_I4_0
                .CreateLabel(out var labelTo0)
                .CreateLabelAt(matcher.Pos + 1, out var labelEnd)
                .Insert(
                    new CodeInstruction(OpCodes.Ldc_I4_0),
                    new CodeInstruction(OpCodes.Bgt_S, labelTo0),
                    new CodeInstruction(OpCodes.Ldc_I4_1),
                    new CodeInstruction(OpCodes.Br_S, labelEnd)
                );

            return matcher.InstructionEnumeration();
        }
    }

    private delegate void ShipEnterWarpState(StationComponent stationComponent, int j);

    private delegate void AddItem(StationComponent stationComponent, ref ShipData shipData);

    private delegate void TakeItem(StationComponent stationComponent, ref int itemId, ref int itemCount, int j);

    private delegate void UpdateStorage(StationComponent stationComponent, int index);

    private delegate void WorkShipBackToIdle(StationComponent stationComponent, int j, ref ShipData shipData);

    private delegate void RematchRemotePairs(StationComponent stationComponent, int index);

    private delegate void SendRematchPacket(StationComponent stationComponent);
}
