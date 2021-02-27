using NebulaClient.MonoBehaviours;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Packets;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaClient
{
    public class PlayerManager
    {
        const int PLAYER_PROTO_ID = 1;
        Dictionary<ushort, Player> remotePlayers;

        public PlayerManager()
        {
            remotePlayers = new Dictionary<ushort, Player>();
        }

        public void AddRemotePlayer(ushort playerId, Movement move)
        {
            if (remotePlayers.ContainsKey(playerId))
            {
                Log.Error($"PlayerManager :: Already contains the playerId {playerId}");
                return;
            }

            Player info = new Player(playerId);

            string playerPrefabPath = LDB.players.Select(PLAYER_PROTO_ID).PrefabPath;
            if (playerPrefabPath != null)
            {
                info.PlayerTransform = Object.Instantiate(Resources.Load<Transform>(playerPrefabPath));
                info.PlayerModelTransform = info.PlayerTransform.Find("Model");

                // Remove local only components
                Object.Destroy(info.PlayerTransform.GetComponent<PlayerFootsteps>());
                Object.Destroy(info.PlayerTransform.GetComponent<PlayerEffect>());
                Object.Destroy(info.PlayerTransform.GetComponent<PlayerAudio>());
                Object.Destroy(info.PlayerTransform.GetComponent<PlayerAnimator>());
                Object.Destroy(info.PlayerTransform.GetComponent<PlayerController>());
                
                info.PlayerTransform.GetComponent<Rigidbody>().isKinematic = true;
                info.Animator = info.PlayerTransform.gameObject.AddComponent<RemotePlayerAnimator>();
            }

            info.PlayerTransform.gameObject.name = $"Remote Player ({playerId})";
            info.PlayerTransform.position = move.Transform.Position.ToUnity();
            info.PlayerTransform.eulerAngles = move.Transform.Rotation.ToUnity();
            info.PlayerTransform.localScale = move.Transform.Scale.ToUnity();
            info.PlayerModelTransform.position = move.ModelTransform.Position.ToUnity();
            info.PlayerModelTransform.eulerAngles = move.ModelTransform.Rotation.ToUnity();
            info.PlayerModelTransform.localScale = move.ModelTransform.Scale.ToUnity();

            remotePlayers.Add(playerId, info);
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
