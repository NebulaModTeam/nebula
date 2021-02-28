using NebulaClient.MonoBehaviours.Remote;
using NebulaModel.Logger;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaClient
{
    public class RemotePlayerManager
    {
        const int PLAYER_PROTO_ID = 1;
        Dictionary<ushort, Player> remotePlayers;

        public RemotePlayerManager()
        {
            remotePlayers = new Dictionary<ushort, Player>();
        }

        public void AddPlayer(ushort playerId)
        {
            if (remotePlayers.ContainsKey(playerId))
            {
                Log.Error($"RemotePlayerManager :: Already contains the playerId {playerId}");
                return;
            }

            Player info = new Player(playerId);

            // Spawn remote player model by cloning the player prefab and replacing local player script by remote player ones.
            string playerPrefabPath = LDB.players.Select(PLAYER_PROTO_ID).PrefabPath;
            if (playerPrefabPath != null)
            {
                info.PlayerTransform = UnityEngine.Object.Instantiate(Resources.Load<Transform>(playerPrefabPath));
                info.PlayerModelTransform = info.PlayerTransform.Find("Model");

                // Remove local player components
                UnityEngine.Object.Destroy(info.PlayerTransform.GetComponent<PlayerFootsteps>());
                UnityEngine.Object.Destroy(info.PlayerTransform.GetComponent<PlayerEffect>());
                UnityEngine.Object.Destroy(info.PlayerTransform.GetComponent<PlayerAudio>());
                UnityEngine.Object.Destroy(info.PlayerTransform.GetComponent<PlayerAnimator>());
                UnityEngine.Object.Destroy(info.PlayerTransform.GetComponent<PlayerController>());
                info.PlayerTransform.GetComponent<Rigidbody>().isKinematic = true;

                // Add remote player components
                info.Movement = info.PlayerTransform.gameObject.AddComponent<RemotePlayerMovement>();
                info.Animator = info.PlayerTransform.gameObject.AddComponent<RemotePlayerAnimation>();
            }

            info.PlayerTransform.gameObject.name = $"Remote Player ({playerId})";

            remotePlayers.Add(playerId, info);
        }

        public void RemovePlayer(ushort playerId)
        {
            if (remotePlayers.ContainsKey(playerId))
            {
                remotePlayers[playerId].Destroy();
                remotePlayers.Remove(playerId);
            }
        }

        public Player GetPlayerById(ushort playerId)
        {
            if (remotePlayers.ContainsKey(playerId))
            {
                return remotePlayers[playerId];
            }
            return null;
        }
    }
}
