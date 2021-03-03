using NebulaModel.Logger;
using System.Collections.Generic;
using UnityEngine;
using NebulaModel.DataStructures;

namespace NebulaClient.GameLogic
{
    public class PlayerManager
    {
        readonly Dictionary<ushort, Player> remotePlayers;
        readonly Dictionary<ushort, RemotePlayerModel> remotePlayerModels;

        public bool IsMasterClient { get; set; } = false;
        public Player LocalPlayer { get; protected set; }
        public readonly LocalPlayerModel LocalPlayerModel = new LocalPlayerModel();

        public PlayerManager()
        {
            remotePlayers = new Dictionary<ushort, Player>();
            remotePlayerModels = new Dictionary<ushort, RemotePlayerModel>();
        }

        public void SetLocalPlayer(ushort localPlayerId)
        {
            LocalPlayer = new Player(localPlayerId);

            // TODO: Only give the player a random color when the join, first check if the player has already played on this server.
            LocalPlayer.UpdateColor(new Float3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
        }

        public void AddRemotePlayer(ushort playerId)
        {
            if (remotePlayers.ContainsKey(playerId))
            {
                Log.Error($"RemotePlayerManager :: Already contains the playerId {playerId}");
                return;
            }

            Player info = new Player(playerId);
            remotePlayers.Add(playerId, info);

            RemotePlayerModel model = new RemotePlayerModel(playerId);
            remotePlayerModels.Add(playerId, model);
        }

        public void RemovePlayer(ushort playerId)
        {
            remotePlayers.Remove(playerId);

            if (remotePlayerModels.ContainsKey(playerId))
            {
                remotePlayerModels[playerId].Destroy();
                remotePlayerModels.Remove(playerId);
            }
        }

        public void RemoveAll()
        {
            foreach (var playerModel in remotePlayerModels.Values)
            {
                playerModel.Destroy();
            }
            remotePlayerModels.Clear();
            remotePlayers.Clear();
        }

        public Player GetPlayerById(ushort playerId)
        {
            if (remotePlayers.ContainsKey(playerId))
            {
                return remotePlayers[playerId];
            }
            return null;
        }

        public RemotePlayerModel GetPlayerModelById(ushort playerId)
        {
            if (remotePlayerModels.ContainsKey(playerId))
            {
                return remotePlayerModels[playerId];
            }
            return null;
        }
    }
}
