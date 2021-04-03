using NebulaModel.DataStructures;
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
        public static Dictionary<PrebuildData, int> prebuildReceivedList { get; set; } = new Dictionary<PrebuildData, int>();
        public static Dictionary<string, bool> PatchLocks { get; set; } = new Dictionary<string, bool>();

        private static INetworkProvider networkProvider;

        public static void SetNetworkProvider(INetworkProvider provider)
        {
            networkProvider = provider;
        }

        public static void SendPacket<T>(T packet) where T : class, new()
        {
            networkProvider?.SendPacket(packet);
        }

        public static void SetReady()
        {
            PatchLocks.Clear();
            PatchLocks.Add("PlanetTransport", false);
            PatchLocks.Add("UIStationWindow", false);

            if (!IsMasterClient)
            {
                // Notify the server that we are done with loading the game
                networkProvider.SendPacket(new SyncComplete());
                InGamePopup.FadeOut();
            }

            // Finally we add the local player components to the player character
            GameMain.mainPlayer.gameObject.AddComponentIfMissing<LocalPlayerMovement>();
            GameMain.mainPlayer.gameObject.AddComponentIfMissing<LocalPlayerAnimation>();
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

            if (!UIRoot.instance.backToMainMenu)
            {
                UIRoot.instance.backToMainMenu = true;
                DSPGame.EndGame();
            }
        }
    }
}
