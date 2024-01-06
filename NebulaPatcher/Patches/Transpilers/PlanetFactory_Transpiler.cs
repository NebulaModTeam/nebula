﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory.Belt;
using NebulaModel.Packets.Factory.Inserter;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(PlanetFactory))]
internal class PlanetFactory_Transpiler
{
    public delegate bool BoundsChecker(PlanetFactory factory, int index);

    public static readonly List<int> CheckPopupPresent = [];
    public static readonly Dictionary<int, List<int>> FaultyVeins = [];

    private static void OnNewSetInserterPickTarget(int inserterId, int otherObjId, int offset, int objId, Vector3 pointPos)
    {
        // 1. Client receives the build request from itself 2. Host sends the build request
        if (Multiplayer.IsActive)
        {
            var factoryManager = Multiplayer.Session.Factories;
            if ((Multiplayer.Session.LocalPlayer.IsHost && !factoryManager.IsIncomingRequest.Value) || Multiplayer.Session.LocalPlayer.Id == factoryManager.PacketAuthor)
            {
                Multiplayer.Session.Network.SendToLocalStar(new NewSetInserterPickTargetPacket(objId, otherObjId, inserterId,
                    offset, pointPos, GameMain.localPlanet?.id ?? -1));
            }
        }
    }

    private static void OnNewSetInserterInsertTarget(int inserterId, int otherObjId, int offset, int objId, Vector3 pointPos)
    {
        if (Multiplayer.IsActive)
        {
            var factoryManager = Multiplayer.Session.Factories;
            if ((Multiplayer.Session.LocalPlayer.IsHost && !factoryManager.IsIncomingRequest.Value) || Multiplayer.Session.LocalPlayer.Id == factoryManager.PacketAuthor)
            {
                Multiplayer.Session.Network.SendToLocalStar(new NewSetInserterInsertTargetPacket(objId, otherObjId,
                    inserterId, offset, pointPos, GameMain.localPlanet?.id ?? -1));
            }
        }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PlanetFactory.OnBeltBuilt))]
    private static IEnumerable<CodeInstruction> OnBeltBuilt_Transpiler(IEnumerable<CodeInstruction> instructions,
        ILGenerator iLGenerator)
    {
        /*
         * Calls
         * OnNewSetInserterPickTarget(objId, pickTarget, inserterId, offset, pointPos);
         * After
         * this.factorySystem.SetInserterPickTarget(inserterId, num6, num5 - num7);
         */
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        var codeMatcher = new CodeMatcher(codeInstructions, iLGenerator)
            .MatchForward(true,
                new CodeMatch(i => i.opcode == OpCodes.Callvirt &&
                                   ((MethodInfo)i.operand).Name == nameof(FactorySystem.SetInserterPickTarget)
                )
            );

        if (codeMatcher.IsInvalid)
        {
            Log.Error("PlanetFactory_Transpiler.OnBeltBuilt 1 failed. Mod version not compatible with game version.");
            return codeInstructions;
        }

        var setInserterTargetInsts = codeMatcher.InstructionsWithOffsets(-5, -1); // inserterId, pickTarget, offset
        var objIdInst = codeMatcher.InstructionAt(-13); // objId
        var pointPosInsts = codeMatcher.InstructionsWithOffsets(8, 10); // pointPos

        codeMatcher = codeMatcher
            .Advance(1)
            .InsertAndAdvance(setInserterTargetInsts.ToArray())
            .InsertAndAdvance(objIdInst)
            .InsertAndAdvance(pointPosInsts.ToArray())
            .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlanetFactory_Transpiler), nameof(OnNewSetInserterPickTarget))));

        /*
         * Calls
         * OnNewSetInserterInsertTarget(objId, pickTarget, inserterId, offset, pointPos);
         * After
         * this.factorySystem.SetInserterInsertTarget(inserterId, num9, num5 - num10);
         */
        codeMatcher = codeMatcher
            .MatchForward(true,
                new CodeMatch(i => i.opcode == OpCodes.Callvirt &&
                                   ((MethodInfo)i.operand).Name == nameof(FactorySystem.SetInserterInsertTarget)
                )
            );

        if (codeMatcher.IsInvalid)
        {
            Log.Error("PlanetFactory_Transpiler.OnBeltBuilt 2 failed. Mod version not compatible with game version.");
            return codeMatcher.InstructionEnumeration();
        }

        setInserterTargetInsts = codeMatcher.InstructionsWithOffsets(-5, -1); // inserterId, pickTarget, offset
        objIdInst = codeMatcher.InstructionAt(-13); // objId
        pointPosInsts = codeMatcher.InstructionsWithOffsets(9, 11); // pointPos

        codeMatcher = codeMatcher
            .Advance(1)
            .InsertAndAdvance(setInserterTargetInsts.ToArray())
            .InsertAndAdvance(objIdInst)
            .InsertAndAdvance(pointPosInsts.ToArray())
            .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PlanetFactory_Transpiler), nameof(OnNewSetInserterInsertTarget))));

        return codeMatcher.InstructionEnumeration();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PlanetFactory.BeltFastFillIn))]
    public static IEnumerable<CodeInstruction> BeltFastFillIn_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var matchCounter = 0;

        var matcher = new CodeMatcher(instructions)
            .MatchForward(true,
                new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "TryInsertItem"))
            .Repeat(localMatcher =>
            {
                localMatcher
                    .Advance(1)
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Ldarg_2),
                        new CodeInstruction(OpCodes.Ldloc, matchCounter == 0 ? 13 : 16),
                        matchCounter == 0 ? new CodeInstruction(OpCodes.Ldloc, 14) : new CodeInstruction(OpCodes.Ldc_I4_1),
                        new CodeInstruction(OpCodes.Conv_U1),
                        new CodeInstruction(OpCodes.Ldloc, matchCounter == 0 ? 15 : 17),
                        new CodeInstruction(OpCodes.Conv_U1))
                    .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<CatchBeltFastFillIn>(
                        (result, factory, beltId, offset, itemId, itemCount, itemInc) =>
                        {
                            if (!Multiplayer.IsActive || !result)
                            {
                                return result;
                            }
                            if (Multiplayer.Session.LocalPlayer.IsHost)
                            {
                                Multiplayer.Session.Network.SendToStar(
                                    new BeltUpdatePutItemOnPacket(beltId, itemId, itemCount, itemInc, factory.planetId),
                                    factory.planet.star.id);
                            }
                            else
                            {
                                Multiplayer.Session.Network.SendToAll(new BeltUpdatePutItemOnPacket(beltId, itemId,
                                    itemCount, itemInc, factory.planetId));
                            }
                            return true;
                        }));
                matchCounter++;
            });
        return matcher.InstructionEnumeration();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PlanetFactory.BeltFastTakeOut))]
    public static IEnumerable<CodeInstruction> BeltFastTakeOut_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions)
            .MatchForward(true,
                new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "RemoveCargoAtIndex"))
            .Repeat(localMatcher =>
            {
                localMatcher
                    .Advance(1)
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Ldloc, 10),
                        new CodeInstruction(OpCodes.Ldloc, 6))
                    .Insert(HarmonyLib.Transpilers.EmitDelegate<CatchBeltFastTakeOut>(
                        (result, factory, beltId, itemId, count) =>
                        {
                            if (!Multiplayer.IsActive)
                            {
                                return result;
                            }
                            var bUpdate = new BeltUpdate[1];

                            bUpdate[0].ItemId = itemId;
                            bUpdate[0].Count = count;
                            bUpdate[0].BeltId = beltId;

                            if (Multiplayer.Session.LocalPlayer.IsHost)
                            {
                                Multiplayer.Session.Network.SendToStar(
                                    new BeltUpdatePickupItemsPacket(bUpdate, factory.planetId), factory.planet.star.id);
                            }
                            else
                            {
                                Multiplayer.Session.Network.SendToAll(
                                    new BeltUpdatePickupItemsPacket(bUpdate, factory.planetId));
                            }
                            return result;
                        }));
            });
        return matcher.InstructionEnumeration();
    }

    private delegate bool CatchBeltFastFillIn(bool result, PlanetFactory factory, int beltId, int offset, int itemId,
        byte itemCount, byte itemInc);

    private delegate bool CatchBeltFastTakeOut(bool result, PlanetFactory factory, int beltId, int itemId, int count);
}
