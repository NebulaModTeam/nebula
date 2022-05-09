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
            using (((NetworkProvider)Multiplayer.Session.Network).PlayerManager.GetConnectedPlayers(out System.Collections.Generic.Dictionary<INebulaConnection, INebulaPlayer> connectedPlayers))
            {
                foreach (System.Collections.Generic.KeyValuePair<INebulaConnection, INebulaPlayer> kvp in connectedPlayers)
                {
                    INebulaConnection connection = kvp.Key;
                    INebulaPlayer player = kvp.Value;
                    if (player.Data.LocalPlanetId == planetId && connection.Equals(originator))
                    {
                        connection.SendPacket(packet);
                    }
                }
            }
        }
    }
}
