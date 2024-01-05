#region

using System.Linq;
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
            var factory = GameMain.galaxy.PlanetById(player.Data.LocalPlanetId)?.factory;
            Vector3 vector;

            DroneManager.RefreshCachedPositions(); // refresh position cache
            if (GameMain.mainPlayer.planetId == player.Data.LocalPlanetId)
            {
                vector = GameMain.mainPlayer.position.normalized * (GameMain.mainPlayer.position.magnitude + 2.8f);
            }
            else
            {
                var playerPos = DroneManager.GetPlayerPosition(player.Id);

                vector = playerPos.normalized * (playerPos.magnitude + 2.8f);
            }

            foreach (var targetObjectId in packet.QueuedEntityIds)
            {
                DroneManager.RemoveBuildRequest(targetObjectId);
                DroneManager.RemovePlayerDronePlan(player.Id, targetObjectId);
                if (factory == null)
                {
                    continue;
                }
                factory.constructionSystem.constructServing.Remove(targetObjectId); // in case it was a construction drone.

                if (GameMain.mainPlayer.planetId == player.Data.LocalPlanetId)
                {
                    for (var i = 1; i < factory.constructionSystem.drones.cursor; i++)
                    {
                        ref var drone = ref factory.constructionSystem.drones.buffer[i];
                        if (drone.owner < 0 && packet.QueuedEntityIds.Contains(drone.targetObjectId))
                        {
                            GameMain.mainPlayer.mecha.constructionModule.RecycleDrone(factory, ref drone);
                        }
                    }
                }

                var entityPos = factory.constructionSystem._obj_hpos(targetObjectId, ref vector);
                var nextClosestPlayer =
                    DroneManager.GetNextClosestPlayerToAfter(player.Data.LocalPlanetId, player.Id, ref entityPos);

                if (nextClosestPlayer == player.Id)
                {
                    continue;
                }
                DroneManager.AddBuildRequest(targetObjectId);
                DroneManager.AddPlayerDronePlan(nextClosestPlayer, targetObjectId);

                // tell players to send out drones
                Multiplayer.Session.Network.SendPacketToPlanet(
                    new NewMechaDroneOrderPacket(player.Data.LocalPlanetId, targetObjectId,
                        nextClosestPlayer, /*TODO: rip*/true), player.Data.LocalPlanetId);
                factory.constructionSystem.constructServing.Add(targetObjectId);

                // only render other drones when on same planet
                if (player.Data.LocalPlanetId == GameMain.mainPlayer.planetId)
                {
                    DroneManager.EjectDronesOfOtherPlayer(nextClosestPlayer, player.Data.LocalPlanetId, targetObjectId);
                }
            }
        }
        else
        {
            // check if there are any drones on the current planet and match the targets from this packet.
            // if so recycle them. overflown drones are handled by RecycleDrone_Postfix
            var factory = GameMain.mainPlayer.factory;

            if (factory == null || GameMain.mainPlayer.planetId != packet.PlanetId)
            {
                return;
            }
            for (var i = 1; i < factory.constructionSystem.drones.cursor; i++)
            {
                ref var drone = ref factory.constructionSystem.drones.buffer[i];
                switch (drone.owner)
                {
                    case <= 0 when packet.QueuedEntityIds.Contains(drone.targetObjectId):
                        GameMain.mainPlayer.mecha.constructionModule.RecycleDrone(factory, ref drone);
                        break;
                    case > 0 when packet.QueuedEntityIds.Contains(drone.targetObjectId):
                        {
                            var battleBaseComponent = factory.defenseSystem.battleBases.buffer[drone.owner];
                            battleBaseComponent.constructionModule.RecycleDrone(factory, ref drone);
                            break;
                        }
                }

                DroneManager.RemoveBuildRequest(drone.targetObjectId);
                factory.constructionSystem.constructServing
                    .Remove(drone.targetObjectId); // in case it was a construction drone.
            }
        }
    }
}
