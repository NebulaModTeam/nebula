using NebulaModel;
using NebulaModel.Networking;
using NebulaWorld;

namespace NebulaNetwork
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
            using (Multiplayer.Session.Network.PlayerManager.GetConnectedPlayers(out var connectedPlayers))
            {
                foreach (var kvp in connectedPlayers)
                {
                    NebulaConnection connection = kvp.Key;
                    NebulaPlayer player = kvp.Value;
                    if (player.Data.LocalPlanetId == planetId && connection != originator)
                    {
                        connection.SendPacket(packet);
                    }
                }
            }
        }
    }
}
