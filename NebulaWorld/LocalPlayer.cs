using NebulaModel.DataStructures;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Session;
using NebulaWorld.MonoBehaviours;
using NebulaWorld.MonoBehaviours.Local;
using System.Collections.Generic;

namespace NebulaWorld
{
    public static class LocalPlayer
    {
        public static bool IsMasterClient { get; set; }
        public static ushort PlayerId => Data.PlayerId;
        public static PlayerData Data { get; private set; }
        public static Dictionary<int, byte[]> PendingFactories { get; set; } = new Dictionary<int, byte[]>();

        private static INetworkProvider networkProvider;

        public static void SetNetworkProvider(INetworkProvider provider)
        {
            networkProvider = provider;
        }

        public static void SendPacket<T>(T packet) where T : class, new()
        {
            networkProvider?.SendPacket(packet);
        }

        public static void SendPacketToLocalStar<T>(T packet) where T : class, new()
        {
            networkProvider?.SendPacketToLocalStar(packet);
        }

        public static void SendPacketToLocalPlanet<T>(T packet) where T : class, new()
        {
            networkProvider?.SendPacketToLocalPlanet(packet);
        }

        public static void SendPacketToPlanet<T>(T packet, int planetId) where T : class, new()
        {
            networkProvider?.SendPacketToPlanet(packet, planetId);
        }

        public static void SendPacketToStar<T>(T packet, int starId) where T : class, new()
        {
            networkProvider?.SendPacketToStar(packet, starId);
        }

        public static void SetReady()
        {

            if (!IsMasterClient)
            {
                // Notify the server that we are done with loading the game
                networkProvider.SendPacket(new SyncComplete());
                InGamePopup.FadeOut();
            }

            // Finally we add the local player components to the player character
            GameMain.mainPlayer.gameObject.AddComponentIfMissing<LocalPlayerMovement>();
            GameMain.mainPlayer.gameObject.AddComponentIfMissing<LocalPlayerAnimation>();

            if (!IsMasterClient)
            {
                //Subscribe for the local star events
                LocalPlayer.SendPacket(new PlayerUpdateLocalStarId(GameMain.data.localStar.id));
            }
        }

        public static void SetPlayerData(PlayerData data)
        {
            Data = data;
        }

        public static void LeaveGame()
        {
            networkProvider.DestroySession();
            PendingFactories.Clear();
            IsMasterClient = false;
            SimulatedWorld.Clear();
            SimulatedWorld.ExitingMultiplayerSession = true;

            if (!UIRoot.instance.backToMainMenu)
            {
                UIRoot.instance.backToMainMenu = true;
                DSPGame.EndGame();
            }
        }
    }
}
