using NebulaModel.Packets;
using UnityEngine;

namespace NebulaClient.MonoBehaviours
{
    public class MultiplayerSession : MonoBehaviour
    {
        public static MultiplayerSession instance;

        public Client Client { get; private set; }
        public RemotePlayerManager RemotePlayerManager { get; private set; }

        private string serverIp;
        private int serverPort;

        void Awake()
        {
            instance = this;

            Client = new Client();
            Client.PacketProcessor.SubscribeReusable<Movement>(OnPlayerMovement);
            Client.PacketProcessor.SubscribeReusable<PlayerAnimationUpdate>(OnPlayerAnimationUpdate);
            Client.PacketProcessor.SubscribeReusable<RemotePlayerJoined>(OnRemotePlayerJoined);
            Client.PacketProcessor.SubscribeReusable<PlayerDisconnected>(OnRemotePlayerDisconnect);
            Client.PacketProcessor.SubscribeReusable<VegeMined>(OnVegeMined);
        }

        public void Connect(string ip, int port)
        {
            serverIp = ip;
            serverPort = port;
            Client.Connect(ip, port);

            RemotePlayerManager = new RemotePlayerManager();
        }

        public void TryToReconnect()
        {
            Disconnect();
            Connect(serverIp, serverPort);
            // TODO: Should freeze game and add a spinner or something during the reconnection.
            // Else the player can still move around during the reconnection procedure which is weird
        }

        public void Disconnect()
        {
            if (Client.IsConnected)
            {
                Client.Disconnect();
            }

            CleanupSession();

        }

        public void LeaveGame()
        {
            Disconnect();

            // Go back to the main menu
            if (!UIRoot.instance.backToMainMenu)
            {
                UIRoot.instance.backToMainMenu = true;
                DSPGame.EndGame();
            }
        }

        void OnDestroy()
        {
            // This make sure to disconnect if you force close the game.
            Disconnect();
        }

        void CleanupSession()
        {
            RemotePlayerManager.RemoveAll();
        }

        void Update()
        {
            Client.Update();
        }

        private void OnRemotePlayerJoined(RemotePlayerJoined packet)
        {
            RemotePlayerManager.AddPlayer(packet.PlayerId);
        }

        private void OnRemotePlayerDisconnect(PlayerDisconnected packet)
        {
            RemotePlayerManager.RemovePlayer(packet.PlayerId);
        }

        private void OnPlayerMovement(Movement packet)
        {
            RemotePlayerManager.GetPlayerById(packet.PlayerId)?.Movement.UpdatePosition(packet);
        }

        private void OnPlayerAnimationUpdate(PlayerAnimationUpdate packet)
        {
            RemotePlayerManager.GetPlayerById(packet.PlayerId)?.Animator.UpdateState(packet);
        }

        private void OnVegeMined(VegeMined packet)
	    {
            GameMain.localPlanet?.factory?.RemoveVegeWithComponents(packet.VegeID);
	    }
    }
}
