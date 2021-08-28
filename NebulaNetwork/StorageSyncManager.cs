using NebulaAPI;
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
            using (((NetworkProvider)Multiplayer.Session.Network).PlayerManager.GetConnectedPlayers(out var connectedPlayers))
            {
                foreach (var kvp in connectedPlayers)
                {
                    NebulaConnection connection = (NebulaConnection)kvp.Key;
                    INebulaPlayer player = kvp.Value;
                    if (player.Data.LocalPlanetId == planetId && connection != originator)
                    {
                        connection.SendPacket(packet);
                    }
                }
            }
        }
    }
}
