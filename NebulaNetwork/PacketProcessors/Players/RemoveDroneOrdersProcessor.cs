#region

using System.Linq;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld.Player;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaNetwork.PacketProcessors.Players;

[RegisterPacketProcessor]
internal class RemoveDroneOrdersProcessor : PacketProcessor<RemoveDroneOrdersPacket>
{
    protected override void ProcessPacket(RemoveDroneOrdersPacket packet, NebulaConnection conn)
    {
        if (packet.QueuedEntityIds == null)
        {
            return;
        }

        if (IsHost)
        {
            // host needs to remove targets from DroneManager
            // but he also needs to RecycleDrone any rendered drone of this player
            // and as clients only send this when they are unable to handle a NewDroneOrder the host should search for the next closest player to ask for construction.
            var player = Multiplayer.Session.Network.PlayerManager.GetPlayer(conn);
            PlanetFactory factory = GameMain.galaxy.PlanetById(player.Data.LocalPlanetId)?.factory;
            Vector3 vector = Vector3.zero;

            DroneManager.ClearCachedPositions(); // refresh position cache
            if (GameMain.mainPlayer.planetId == player.Data.LocalPlanetId)
            {
                vector = GameMain.mainPlayer.position.normalized * (GameMain.mainPlayer.position.magnitude + 2.8f);
            }
            else
            {
                Vector3 playerPos = DroneManager.GetPlayerPosition(player.Id);

                vector = playerPos.normalized * (playerPos.magnitude + 2.8f);
            }

            foreach (int targetObjectId in packet.QueuedEntityIds)
            {
                DroneManager.RemoveBuildRequest(targetObjectId);
                DroneManager.RemovePlayerDronePlan(player.Id, targetObjectId);
                factory.constructionSystem.constructServing.Remove(targetObjectId); // in case it was a construction drone.

                if (GameMain.mainPlayer.planetId == player.Data.LocalPlanetId)
                {
                    if (factory != null)
                    {
                        for (int i = 1; i < factory.constructionSystem.drones.cursor; i++)
                        {
                            ref DroneComponent drone = ref factory.constructionSystem.drones.buffer[i];
                            if (drone.owner < 0 && packet.QueuedEntityIds.Contains(drone.targetObjectId))
                            {
                                GameMain.mainPlayer.mecha.constructionModule.RecycleDrone(factory, ref drone);
                            }
                        }
                    }
                }

                Vector3 entityPos = factory.constructionSystem._obj_hpos(targetObjectId, ref vector);
                var nextClosestPlayer = DroneManager.GetNextClosestPlayerToAfter(player.Data.LocalPlanetId, player.Id, ref entityPos);

                if (nextClosestPlayer != player.Id)
                {
                    DroneManager.AddBuildRequest(targetObjectId);
                    DroneManager.AddPlayerDronePlan(nextClosestPlayer, targetObjectId);

                    // tell players to send out drones
                    Multiplayer.Session.Network.SendPacketToPlanet(new NewMechaDroneOrderPacket(player.Data.LocalPlanetId, targetObjectId, nextClosestPlayer, /*TODO: rip*/true), player.Data.LocalPlanetId);
                    factory.constructionSystem.constructServing.Add(targetObjectId);

                    // only render other drones when on same planet
                    if (player.Data.LocalPlanetId == GameMain.mainPlayer.planetId)
                    {
                        DroneManager.EjectDronesOfOtherPlayer(nextClosestPlayer, player.Data.LocalPlanetId, targetObjectId);
                    }
                }
            }
        }
        else
        {
            // check if there are any drones on the current planet and match the targets from this packet.
            // if so recycle them. overflown drones are handled by RecycleDrone_Postfix
            PlanetFactory factory = GameMain.mainPlayer.factory;
            var player = Multiplayer.Session.Network.PlayerManager.GetPlayer(conn);

            if (factory != null && GameMain.mainPlayer.planetId == player.Data.LocalPlanetId)
            {
                for (int i = 1; i < factory.constructionSystem.drones.cursor; i++)
                {
                    ref DroneComponent drone = ref factory.constructionSystem.drones.buffer[i];
                    if (drone.owner <= 0 && packet.QueuedEntityIds.Contains(drone.targetObjectId))
                    {
                        GameMain.mainPlayer.mecha.constructionModule.RecycleDrone(factory, ref drone);
                    }
                    else if (drone.owner > 0 && packet.QueuedEntityIds.Contains(drone.targetObjectId))
                    {
                        BattleBaseComponent battleBaseComponent = factory.defenseSystem.battleBases.buffer[drone.owner];
                        battleBaseComponent.constructionModule.RecycleDrone(factory, ref drone);
                    }

                    DroneManager.RemoveBuildRequest(drone.targetObjectId);
                    factory.constructionSystem.constructServing.Remove(drone.targetObjectId); // in case it was a construction drone.
                }
            }
        }
    }
}
