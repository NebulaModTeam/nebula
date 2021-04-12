using NebulaModel.Networking;
using System.Collections.Generic;

namespace NebulaHost
{
    public static class StorageSyncManager
    {
        public static void SendToPlayersOnTheSamePlanet<T>(T packet, int planetId) where T : class, new()
        {
            SendToOtherPlayersOnTheSamePlanet(null, packet, planetId);
        }

        public static void SendToOtherPlayersOnTheSamePlanet<T>(NebulaConnection originator, T packet, int planetId) where T : class, new()
        {
            //Send to players on the same planet
            foreach (KeyValuePair<NebulaConnection, Player> player in MultiplayerHostSession.Instance.PlayerManager.ConnectedPlayers)
            {
                if (player.Value.Data.LocalPlanetId == planetId && player.Key != originator)
                {
                    player.Key.SendPacket(packet);
                }
            }
        }
    }
}
