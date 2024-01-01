using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Players;
using NebulaWorld;
using NebulaWorld.Player;
using UnityEngine;

namespace NebulaPatcher.Patches.Transpilers
{
    delegate bool EjectMechaDroneLocalOrRemote(ConstructionModuleComponent constructionModuleComponent, PlanetFactory factory, Player player, int targetConstructionObjectId, int targetRepairObjectId, bool priority);
    delegate bool UseThisTarget(bool alreadyContained, int targetConstructionObjectId);

    [HarmonyPatch(typeof(ConstructionModuleComponent))]
    internal class ConstructionModuleComponent_Transpiler
    {
        static bool EjectMechaDroneLocalOrRemote(PlanetFactory factory, Player player, int targetObjectId, bool priorConstruct)
        {
            return true;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(ConstructionModuleComponent.IdleDroneProcedure))]
        public static IEnumerable<CodeInstruction> IdleDroneProcedure_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            CodeMatcher matcher = new CodeMatcher(instructions, il);

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
                return instructions;
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
                    HarmonyLib.Transpilers.EmitDelegate<EjectMechaDroneLocalOrRemote>((ConstructionModuleComponent constructionModuleComponent, PlanetFactory factory, Player player, int targetConstructionObjectId, int targetRepairObjectId, bool priority) =>
                    {
                        if (!Multiplayer.IsActive)
                        {
                            return true;
                        }

                        // one of these must be != 0 because original method checks for it.
                        int targetObjectId = 0;
                        if (targetConstructionObjectId != 0) targetObjectId = targetConstructionObjectId;
                        if (targetRepairObjectId != 0) targetObjectId = targetRepairObjectId;

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

                            Multiplayer.Session.Network.SendPacket(new NewDroneOrderPacket(GameMain.mainPlayer.planetId, targetObjectId, Multiplayer.Session.LocalPlayer.Id, priority));
                        }
                        else
                        {
                            // if we are the host we can directly determine in here who should send out drones.
                            DroneManager.ClearCachedPositions(); // refresh position cache

                            Vector3 vector = GameMain.mainPlayer.position.normalized * (GameMain.mainPlayer.position.magnitude + 2.8f);
                            Vector3 entityPos = factory.constructionSystem._obj_hpos(targetObjectId, ref vector);
                            ushort closestPlayer = DroneManager.GetClosestPlayerTo(GameMain.mainPlayer.planetId, ref entityPos);

                            // send out drones if we are closest
                            if (closestPlayer == Multiplayer.Session.LocalPlayer.Id)
                            {
                                DroneManager.AddBuildRequest(targetObjectId);
                                DroneManager.AddPlayerDronePlan(closestPlayer, targetObjectId);

                                // tell players to send out drones only if we are the closest one. otherwise players will ask themselve in case they are able to send out drones.
                                Multiplayer.Session.Network.SendPacketToPlanet(new NewDroneOrderPacket(GameMain.mainPlayer.planetId, targetObjectId, closestPlayer, priority), GameMain.mainPlayer.planetId);

                                GameMain.mainPlayer.mecha.constructionModule.EjectMechaDrone(factory, GameMain.mainPlayer, targetObjectId, priority);
                            }
                        }

                        return false;

                    }),
                    new CodeInstruction(OpCodes.Brtrue, jmpToOriginalCode),
                    new CodeInstruction(OpCodes.Ret));

            return matcher.InstructionEnumeration();
        }

        // skip targets that we already asked the host about, but only ask as much as we can handle with our idle drones.
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(ConstructionModuleComponent.SearchForNewTargets))]
        public static IEnumerable<CodeInstruction> SearchForNewTargets_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);

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
                return instructions;
            }

            matcher
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldloc_S, 28),
                    HarmonyLib.Transpilers.EmitDelegate<UseThisTarget>((bool alreadyContained, int targetConstructionObjectId) =>
                    {
                        if (!Multiplayer.IsActive)
                        {
                            return alreadyContained;
                        }

                        // returning true here means the targetConstructionObjectId will not be selected as a valid next target
                        return alreadyContained || DroneManager.IsPendingBuildRequest(targetConstructionObjectId) || DroneManager.CountPendingBuildRequest() > GameMain.mainPlayer.mecha.constructionModule.droneCount;
                    }));

            return matcher.InstructionEnumeration();
        }
    }
}
