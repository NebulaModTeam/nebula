using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory.BattleBase;
using NebulaModel.Packets.Players;
using NebulaWorld;
using NebulaWorld.Player;

namespace NebulaPatcher.Patches.Transpilers;

internal delegate bool EjectMechaDroneLocalOrRemote(ConstructionModuleComponent constructionModuleComponent,
    PlanetFactory factory, Player player, int targetConstructionObjectId, int targetRepairObjectId, bool priority);

internal delegate void BroadcastEjectBattleBaseDrone(ConstructionModuleComponent _this, PlanetFactory factory,
    ref DroneComponent drone, ref CraftData cData, int targetId);

internal delegate bool UseThisTarget(bool alreadyContained, int targetConstructionObjectId);

[HarmonyPatch(typeof(ConstructionModuleComponent))]
internal class ConstructionModuleComponent_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ConstructionModuleComponent.IdleDroneProcedure))]
    public static IEnumerable<CodeInstruction> IdleDroneProcedure_Transpiler(IEnumerable<CodeInstruction> instructions,
        ILGenerator il)
    {
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        var matcher = new CodeMatcher(codeInstructions, il);

        matcher
            .MatchForward(true,
                new CodeMatch(OpCodes.Add),
                new CodeMatch(OpCodes.Conv_I4),
                new CodeMatch(OpCodes.Stloc_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Clt),
                new CodeMatch(OpCodes.Stloc_S),
                new CodeMatch(OpCodes.Ldloc_S));

        if (matcher.IsInvalid)
        {
            Log.Error(
                "ConstructionModuleComponent_Transpiler.IdleDroneProcedure_Transpiler 1 failed. Mod version not compatible with game version.");
            return codeInstructions;
        }

        matcher.CreateLabel(out var jmpToOriginalCode);

        /*
         * What this does:
         if (player.mecha.CheckEjectConstructionDroneCondition())
        {
            int num3 = this.droneCount - this.droneIdleCount;
            int num4 = (int)((double)((float)this.droneCount * this.dronePriorConstructRatio) + 0.5);
            bool flag2 = num3 < num4;
            // inject code here, asking host if we can eject drones or if someone else will do so.
         */
        matcher
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Ldloc_S, 5),
                HarmonyLib.Transpilers.EmitDelegate<EjectMechaDroneLocalOrRemote>(
                    (constructionModuleComponent, factory, player, targetConstructionObjectId, targetRepairObjectId,
                        priority) =>
                    {
                        if (!Multiplayer.IsActive)
                        {
                            return true;
                        }

                        // one of these must be != 0 because original method checks for it.
                        var targetObjectId = 0;
                        if (targetConstructionObjectId != 0)
                        {
                            targetObjectId = targetConstructionObjectId;
                        }
                        if (targetRepairObjectId != 0)
                        {
                            targetObjectId = targetRepairObjectId;
                        }

                        if (DroneManager.IsPendingBuildRequest(targetObjectId))
                        {
                            return true;
                        }

                        // clients need to ask host if they want to send out drones. drones will only be send out in response to that, but not here.
                        // clients use the DroneManager.PendingBuildRequests to remember which targetObjectId were already asked about.
                        if (Multiplayer.Session.LocalPlayer.IsClient)
                        {
                            // decrease idle drones to avoid sending more requests as we have drones. They are replenished once receiving the response.
                            //GameMain.mainPlayer.mecha.constructionModule.droneIdleCount--;
                            DroneManager.AddBuildRequest(targetObjectId);

                            Multiplayer.Session.Network.SendPacket(new NewMechaDroneOrderPacket(GameMain.mainPlayer.planetId,
                                targetObjectId, Multiplayer.Session.LocalPlayer.Id, priority));
                        }
                        else
                        {
                            // if we are the host we can directly determine in here who should send out drones.
                            DroneManager.RefreshCachedPositions(); // refresh position cache

                            var vector = GameMain.mainPlayer.position.normalized *
                                         (GameMain.mainPlayer.position.magnitude + 2.8f);
                            var entityPos = factory.constructionSystem._obj_hpos(targetObjectId, ref vector);
                            var closestPlayer = DroneManager.GetClosestPlayerTo(GameMain.mainPlayer.planetId, ref entityPos);

                            // send out drones if we are closest
                            if (closestPlayer != Multiplayer.Session.LocalPlayer.Id)
                            {
                                return false;
                            }
                            DroneManager.AddBuildRequest(targetObjectId);
                            DroneManager.AddPlayerDronePlan(closestPlayer, targetObjectId);

                            // tell players to send out drones only if we are the closest one. otherwise players will ask themselve in case they are able to send out drones.
                            Multiplayer.Session.Network.SendPacketToPlanet(
                                new NewMechaDroneOrderPacket(GameMain.mainPlayer.planetId, targetObjectId, closestPlayer,
                                    priority), GameMain.mainPlayer.planetId);

                            GameMain.mainPlayer.mecha.constructionModule.EjectMechaDrone(factory, GameMain.mainPlayer,
                                targetObjectId, priority);
                            factory.constructionSystem.constructServing.Add(targetObjectId);
                        }

                        return false;
                    }),
                new CodeInstruction(OpCodes.Brtrue, jmpToOriginalCode),
                new CodeInstruction(OpCodes.Ret));

        // Now search for the parts where BattleBases eject their drones.
        // The host must sync this to clients. (only host runs this because of a prefix patch that skips it for clients.
        // add code before each 'this.EjectBaseDrone(factory, ref ptr3, ref ptr4, num/num2);' to broadcast to planets
        // do this here to distinguish construction and repairing

        var pos = matcher.Pos;

        // construction
        matcher
            .MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldarg_1),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldloc_1), // this is construction
                new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "EjectBaseDrone"));

        if (matcher.IsInvalid)
        {
            Log.Error(
                "ConstructionModuleComponent_Transpiler.IdleDroneProcedure_Transpiler 2 failed. Mod version not compatible with game version.");
            return codeInstructions;
        }

        matcher
            .Repeat(matcher =>
            {
                matcher
                    .InsertAndAdvance(
                        HarmonyLib.Transpilers.EmitDelegate<BroadcastEjectBattleBaseDrone>(
                            (ConstructionModuleComponent _this, PlanetFactory factory, ref DroneComponent drone,
                                ref CraftData cData, int targetId) =>
                            {
                                if (!Multiplayer.IsActive)
                                {
                                    _this.EjectBaseDrone(factory, ref drone, ref cData, targetId);
                                    return;
                                }
                                if (DroneManager.IsPendingBuildRequest(targetId))
                                {
                                    return;
                                }
                                DroneManager.AddBuildRequest(
                                    targetId); // so clients will not receive any mecha drone order for this entity
                                Multiplayer.Session.Network.SendPacketToPlanet(
                                    new NewBattleBaseDroneOrderPacket(factory.planetId, targetId, _this.id, true),
                                    factory.planetId);
                                _this.EjectBaseDrone(factory, ref drone, ref cData, targetId);
                                factory.constructionSystem.constructServing.Add(targetId);
                            }))
                    .Set(OpCodes.Nop, null); // remove original call
            });

        // because matcher.Back() did not find the code for some reason...
        while (matcher.Pos != pos)
        {
            matcher.Advance(-1);
        }

        // repairment
        matcher
            .MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldarg_1),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldloc_2), // this is repair
                new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "EjectBaseDrone"));

        if (matcher.IsInvalid)
        {
            Log.Error(
                "ConstructionModuleComponent_Transpiler.IdleDroneProcedure_Transpiler 3 failed. Mod version not compatible with game version.");
            return codeInstructions;
        }

        matcher
            .Repeat(matcher =>
            {
                matcher
                    .InsertAndAdvance(
                        HarmonyLib.Transpilers.EmitDelegate<BroadcastEjectBattleBaseDrone>(
                            (ConstructionModuleComponent _this, PlanetFactory factory, ref DroneComponent drone,
                                ref CraftData cData, int targetId) =>
                            {
                                if (!Multiplayer.IsActive)
                                {
                                    _this.EjectBaseDrone(factory, ref drone, ref cData, targetId);
                                    return;
                                }
                                if (!DroneManager.IsPendingBuildRequest(targetId))
                                {
                                    DroneManager.AddBuildRequest(
                                        targetId); // so clients will not receive any mecha drone order for this entity
                                    Multiplayer.Session.Network.SendPacketToPlanet(
                                        new NewBattleBaseDroneOrderPacket(factory.planetId, targetId, _this.id, false),
                                        factory.planetId);
                                    _this.EjectBaseDrone(factory, ref drone, ref cData, targetId);
                                }
                            }))
                    .Set(OpCodes.Nop, null); // remove original call
            });

        return matcher.InstructionEnumeration();
    }

    // skip targets that we already asked the host about, but only ask as much as we can handle with our idle drones.
    // replace: if (!factory.constructionSystem.constructServing.Contains(num12))
    // with the checks from below
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ConstructionModuleComponent.SearchForNewTargets))]
    public static IEnumerable<CodeInstruction> SearchForNewTargets_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        var matcher = new CodeMatcher(codeInstructions);

        matcher
            .MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_1),
                new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "constructionSystem"),
                new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "constructServing"),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "Contains"),
                new CodeMatch(OpCodes.Brtrue));

        if (matcher.IsInvalid)
        {
            Log.Error(
                "ConstructionModuleComponent_Transpiler.SearchForNewTargets_Transpiler 1 failed. Mod version not compatible with game version.");
            return codeInstructions;
        }

        matcher
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldloc_S, 28),
                HarmonyLib.Transpilers.EmitDelegate<UseThisTarget>((alreadyContained, targetConstructionObjectId) =>
                {
                    if (!Multiplayer.IsActive)
                    {
                        return alreadyContained;
                    }

                    var myDronePlansCount = DroneManager.GetPlayerDronePlansCount(Multiplayer.Session.LocalPlayer.Id);

                    var isPendingBuildRequest = DroneManager.IsPendingBuildRequest(targetConstructionObjectId);
                    var clientNoIdleDrones = Multiplayer.Session.LocalPlayer.IsClient &&
                                             DroneManager.CountPendingBuildRequest() >
                                             GameMain.mainPlayer.mecha.constructionModule.droneCount;
                    var hostNoIdleDrones = Multiplayer.Session.LocalPlayer.IsHost && myDronePlansCount >
                        GameMain.mainPlayer.mecha.constructionModule.droneCount;

                    if (!alreadyContained && isPendingBuildRequest && Multiplayer.Session.LocalPlayer.IsHost)
                    {
                        // this seems to be a deadlock desync, but a should be the correct value
                        DroneManager.RemoveBuildRequest(targetConstructionObjectId);
                    }

                    // returning true here means the targetConstructionObjectId will not be selected as a valid next target
                    return alreadyContained || isPendingBuildRequest || clientNoIdleDrones || hostNoIdleDrones;
                }));

        return matcher.InstructionEnumeration();
    }
}
