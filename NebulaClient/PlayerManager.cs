using System.Collections.Generic;
using UnityEngine;

namespace NebulaClient
{
    public class PlayerManager
    {
        Dictionary<ushort, Player> remotePlayers;

        public PlayerManager()
        {
            remotePlayers = new Dictionary<ushort, Player>();
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
