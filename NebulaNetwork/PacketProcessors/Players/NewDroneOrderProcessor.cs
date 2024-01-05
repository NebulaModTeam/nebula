#region

using System.Net.Sockets;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;
using NebulaWorld.Player;
using UnityEngine;

#endregion

namespace NebulaNetwork.PacketProcessors.Players;

[RegisterPacketProcessor]
internal class NewDroneOrderProcessor : PacketProcessor<NewMechaDroneOrderPacket>
{
    protected override void ProcessPacket(NewMechaDroneOrderPacket packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            // Host needs to determine who is closest and who should send out drones.
            // clients only send out construction drones in response to this packet.

            DroneManager.RefreshCachedPositions();
            Vector3 initialVector = getInitialVector(packet);

            var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
            if (factory == null)
            {
                return;
            }
            var entityPos = factory.constructionSystem._obj_hpos(packet.EntityId, ref initialVector);
            var closestPlayer = DroneManager.GetClosestPlayerTo(packet.PlanetId, ref entityPos);

            // only one drone per building allowed
            if (DroneManager.IsPendingBuildRequest(packet.EntityId))
            {
                return;
            }
            if (closestPlayer == Multiplayer.Session.LocalPlayer.Id &&
                GameMain.mainPlayer.mecha.constructionModule.droneIdleCount > 0 &&
                GameMain.mainPlayer.mecha.constructionModule.droneEnabled)
            {
                informAndEjectLocalDrones(packet, factory, closestPlayer);
            }
            else if (closestPlayer == Multiplayer.Session.LocalPlayer.Id)
            {
                // we are closest one but we do not have enough drones or they are disabled, so search next closest player
                var nextClosestPlayer =
                    DroneManager.GetNextClosestPlayerToAfter(packet.PlanetId, closestPlayer, ref entityPos);
                if (nextClosestPlayer == closestPlayer)
                {
                    // there is no other one to ask so wait and do nothing.
                    return;
                }

                informAndEjectRemoteDrones(packet, factory, nextClosestPlayer);
            }
            else if (closestPlayer != Multiplayer.Session.LocalPlayer.Id)
            {
                informAndEjectRemoteDrones(packet, factory, closestPlayer);
            }
        }
        else
        {
            var elected = packet.PlayerId == Multiplayer.Session.LocalPlayer.Id;
            var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;

            switch (elected)
            {
                case false:
                    {
                        // remove drone order request if not elected.
                        DroneManager.RemoveBuildRequest(packet.EntityId);

                        // only render other drones when on same planet
                        if (packet.PlanetId != GameMain.mainPlayer.planetId)
                        {
                            return;
                        }

                        // now spawn drones of other player, this is only visual to avoid having buildings popping up without any construction drone.
                        if (factory != null)
                        {
                            DroneManager.EjectDronesOfOtherPlayer(packet.PlayerId, packet.PlanetId, packet.EntityId);
                            factory.constructionSystem.constructServing.Add(packet.EntityId);
                        }
                        break;
                    }
                case true when GameMain.mainPlayer.mecha.constructionModule.droneIdleCount > 0 && GameMain.mainPlayer.mecha.constructionModule.droneEnabled:
                    {
                        // we should send out drones, so do it.

                        if (factory != null)
                        {
                            GameMain.mainPlayer.mecha.constructionModule.EjectMechaDrone(factory, GameMain.mainPlayer,
                                packet.EntityId,
                                packet.Priority);
                            factory.constructionSystem.constructServing.Add(packet.EntityId);
                        }
                        break;
                    }
                case true when GameMain.mainPlayer.mecha.constructionModule.droneIdleCount <= 0 || !GameMain.mainPlayer.mecha.constructionModule.droneEnabled:
                    // remove drone order request if we cant handle it
                    DroneManager.RemoveBuildRequest(packet.EntityId);

                    // others need to remove drones that are rendered for us.
                    Multiplayer.Session.Network.SendPacketToLocalPlanet(new RemoveDroneOrdersPacket([packet.EntityId], packet.PlanetId));
                    break;
            }

            // TODO: what about these from IdleDroneProcedure()

            /*
            ref EntityData ptr = ref factory.entityPool[num2];
            CombatStat[] buffer = factory.skillSystem.combatStats.buffer;
            int combatStatId = ptr.combatStatId;
            buffer[combatStatId].repairerCount = buffer[combatStatId].repairerCount + 1;
             */
        }
    }

    private Vector3 getInitialVector(NewMechaDroneOrderPacket packet)
    {
        Vector3 vector;

        if (GameMain.mainPlayer.planetId == packet.PlanetId)
        {
            vector = GameMain.mainPlayer.position.normalized * (GameMain.mainPlayer.position.magnitude + 2.8f);
        }
        else
        {
            var playerPos = DroneManager.GetPlayerPosition(packet.PlayerId);

            vector = playerPos.normalized * (playerPos.magnitude + 2.8f);
        }

        return vector;
    }

    private void informAndEjectRemoteDrones(NewMechaDroneOrderPacket packet, PlanetFactory factory, ushort closestPlayerId)
    {
        DroneManager.AddBuildRequest(packet.EntityId);
        DroneManager.AddPlayerDronePlan(closestPlayerId, packet.EntityId);

        // tell players to send out drones
        Multiplayer.Session.Network.SendPacketToPlanet(
        new NewMechaDroneOrderPacket(packet.PlanetId, packet.EntityId, closestPlayerId, packet.Priority),
            packet.PlanetId);
        factory.constructionSystem.constructServing.Add(packet.EntityId);

        // only render other drones when on same planet
        if (packet.PlanetId == GameMain.mainPlayer.planetId)
        {
            DroneManager.EjectDronesOfOtherPlayer(closestPlayerId, packet.PlanetId, packet.EntityId);
        }
    }

    private void informAndEjectLocalDrones(NewMechaDroneOrderPacket packet, PlanetFactory factory, ushort closestPlayerId)
    {
        DroneManager.AddBuildRequest(packet.EntityId);
        DroneManager.AddPlayerDronePlan(closestPlayerId, packet.EntityId);

        // tell players to send out drones
        Multiplayer.Session.Network.SendPacketToPlanet(
            new NewMechaDroneOrderPacket(packet.PlanetId, packet.EntityId, closestPlayerId, packet.Priority),
            packet.PlanetId);

        GameMain.mainPlayer.mecha.constructionModule.EjectMechaDrone(factory, GameMain.mainPlayer, packet.EntityId,
            packet.Priority);
        factory.constructionSystem.constructServing.Add(packet.EntityId);
    }
}
