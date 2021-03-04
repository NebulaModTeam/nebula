using NebulaModel.Packets.Players;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaWorld
{
    /// <summary>
    /// This class keeps track of our simulated world. It holds all temporary entities like remote player models 
    /// and also helps to execute some remote player actions that you would want to replicate on the local client.
    /// </summary>
    public static class SimulatedWorld
    {
        // TODO: Keep track here of any additional multiplayer stuff that gets created that is not really there in the single player game.
        static Dictionary<ushort, RemotePlayerModel> remotePlayersModels;

        public static void Initialize()
        {
            remotePlayersModels = new Dictionary<ushort, RemotePlayerModel>();
        }

        public static void SpawnRemotePlayerModel(ushort playerId)
        {
            RemotePlayerModel model = new RemotePlayerModel(playerId);
            remotePlayersModels.Add(playerId, model);
        }

        public static void DestroyRemotePlayerModel(ushort playerId)
        {
            if (remotePlayersModels.TryGetValue(playerId, out RemotePlayerModel player))
            {
                player.Destroy();
                remotePlayersModels.Remove(playerId);
            }
        }

        public static void UpdateRemotePlayerPosition(PlayerMovement packet)
        {
            if (remotePlayersModels.TryGetValue(packet.PlayerId, out RemotePlayerModel player))
            {
                player.Movement.UpdatePosition(packet);
            }
        }

        public static void UpdateRemotePlayerAnimation(PlayerAnimationUpdate packet)
        {
            if (remotePlayersModels.TryGetValue(packet.PlayerId, out RemotePlayerModel player))
            {
                player.Animator.UpdateState(packet);
            }
        }

        public static void UpdatePlayerColor(ushort playerId, Color color)
        {
            RemotePlayerModel player;
            if (!remotePlayersModels.TryGetValue(playerId, out player))
                return;

            // Apply new color to each part of the mecha
            Renderer[] componentsInChildren = player.PlayerTransform.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
            foreach (Renderer r in componentsInChildren)
            {
                if (r.material?.name == "icarus-armor (Instance)")
                {
                    r.material.SetColor("_Color", color);
                }
            }
        }

        public static void MineVegetable(int vegetationId, bool isVegetation, int planetId)
        {
            // TODO: 
        }

    }
}
