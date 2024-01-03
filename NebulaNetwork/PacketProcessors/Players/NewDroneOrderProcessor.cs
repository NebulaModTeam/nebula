#region

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
        //todo: break up into smaller methods
        if (IsHost)
        {
            // Host needs to determine who is closest and who should send out drones.
            // clients only send out construction drones in response to this packet.

            DroneManager.ClearCachedPositions(); // refresh position cache
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

            var factory = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory;
            if (factory == null)
            {
                return;
            }
            var entityPos = factory.constructionSystem._obj_hpos(packet.EntityId, ref vector);
            var closestPlayer = DroneManager.GetClosestPlayerTo(packet.PlanetId, ref entityPos);

            // only one drone per building allowed
            if (DroneManager.IsPendingBuildRequest(packet.EntityId))
            {
                return;
            }
            if (closestPlayer == Multiplayer.Session.LocalPlayer.Id &&
                GameMain.mainPlayer.mecha.constructionModule.droneIdleCount > 0)
            {
                DroneManager.AddBuildRequest(packet.EntityId);
                DroneManager.AddPlayerDronePlan(closestPlayer, packet.EntityId);

                // tell players to send out drones
                Multiplayer.Session.Network.SendPacketToPlanet(
                    new NewMechaDroneOrderPacket(packet.PlanetId, packet.EntityId, closestPlayer, packet.Priority),
                    packet.PlanetId);

                GameMain.mainPlayer.mecha.constructionModule.EjectMechaDrone(factory, GameMain.mainPlayer, packet.EntityId,
                    packet.Priority);
                factory.constructionSystem.constructServing.Add(packet.EntityId);
            }
            else if (closestPlayer == Multiplayer.Session.LocalPlayer.Id)
            {
                // we are closest one but we do not have enough drones, so search next closest player
                var nextClosestPlayer =
                    DroneManager.GetNextClosestPlayerToAfter(packet.PlanetId, closestPlayer, ref entityPos);
                if (nextClosestPlayer == closestPlayer)
                {
                    // there is no other one to ask so wait and do nothing.
                    return;
                }
                DroneManager.AddBuildRequest(packet.EntityId);
                DroneManager.AddPlayerDronePlan(nextClosestPlayer, packet.EntityId);

                // tell players to send out drones
                Multiplayer.Session.Network.SendPacketToPlanet(
                    new NewMechaDroneOrderPacket(packet.PlanetId, packet.EntityId, nextClosestPlayer, packet.Priority),
                    packet.PlanetId);
                factory.constructionSystem.constructServing.Add(packet.EntityId);

                // only render other drones when on same planet
                if (packet.PlanetId == GameMain.mainPlayer.planetId)
                {
                    DroneManager.EjectDronesOfOtherPlayer(nextClosestPlayer, packet.PlanetId, packet.EntityId);
                }
            }
            else if (closestPlayer != Multiplayer.Session.LocalPlayer.Id)
            {
                DroneManager.AddBuildRequest(packet.EntityId);
                DroneManager.AddPlayerDronePlan(closestPlayer, packet.EntityId);

                // tell players to send out drones
                Multiplayer.Session.Network.SendPacketToPlanet(
                    new NewMechaDroneOrderPacket(packet.PlanetId, packet.EntityId, closestPlayer, packet.Priority),
                    packet.PlanetId);
                factory.constructionSystem.constructServing.Add(packet.EntityId);

                // only render other drones when on same planet
                if (packet.PlanetId == GameMain.mainPlayer.planetId)
                {
                    DroneManager.EjectDronesOfOtherPlayer(closestPlayer, packet.PlanetId, packet.EntityId);
                }
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
                case true when GameMain.mainPlayer.mecha.constructionModule.droneIdleCount > 0:
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
                case true when GameMain.mainPlayer.mecha.constructionModule.droneIdleCount <= 0:
                    // remove drone order request if we cant handle it
                    DroneManager.RemoveBuildRequest(packet.EntityId);

                    // others need to remove drones that are rendered for us.
                    Multiplayer.Session.Network.SendPacketToLocalPlanet(new RemoveDroneOrdersPacket([packet.EntityId]));
                    break;
            }

            // TODO: what about these from IdleDroneProcedure() ?? currently we do checks in SearchForNewTargets_Transpiler() so we dont really need to add to factory.constructionSystem.constructServing afaik

            // factory.constructionSystem.constructServing.Add(num);
            /*
            ref EntityData ptr = ref factory.entityPool[num2];
            CombatStat[] buffer = factory.skillSystem.combatStats.buffer;
            int combatStatId = ptr.combatStatId;
            buffer[combatStatId].repairerCount = buffer[combatStatId].repairerCount + 1;
             */
        }
    }
}
