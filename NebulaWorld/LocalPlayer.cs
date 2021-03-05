using LiteNetLib;
using NebulaModel.DataStructures;
using NebulaModel.Packets.Session;

namespace NebulaWorld
{
    public static class LocalPlayer
    {
        public static bool IsMasterClient { get; set; }
        public static ushort PlayerId => Data.PlayerId;
        public static PlayerData Data { get; private set; }

        private static INetworkProvider networkProvider;

        public static void SetNetworkProvider(INetworkProvider provider)
        {
            networkProvider = provider;
        }

        public static void SendPacket<T>(T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, new()
        {
            networkProvider?.SendPacket(packet, deliveryMethod);
        }

        public static void SetReady()
        {
            if (!IsMasterClient)
            {
                // Notify the server that we are done with loading the game
                networkProvider.SendPacket(new SyncComplete());
                InGamePopup.FadeOut();
            }
        }

        public static void SetPlayerData(PlayerData data)
        {
            Data = data;
            GameMain.mainPlayer.transform.position = data.Position.ToUnity();
            GameMain.mainPlayer.transform.eulerAngles = data.Rotation.ToUnity();
            
            // Don't update color, only do it when the game finished loading
            // SimulatedWorld.UpdatePlayerColor(data.PlayerId, data.Color);
        }

        public static void LeaveGame()
        {
            networkProvider.DestroySession();
            SimulatedWorld.Clear();

            if (!UIRoot.instance.backToMainMenu)
            {
                UIRoot.instance.backToMainMenu = true;
                DSPGame.EndGame();
            }
        }
    }
}
