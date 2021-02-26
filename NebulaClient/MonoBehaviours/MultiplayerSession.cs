using NebulaModel.Networking;
using NebulaModel.Packets;
using UnityEngine;

namespace NebulaClient.MonoBehaviours
{
    public class MultiplayerSession : MonoBehaviour
    {
        public Client Client { get; private set; }
        public PlayerManager PlayerManager { get; private set; }

        public static MultiplayerSession instance;

        void Awake()
        {
            instance = this;
        }

        public void Connect(string ip, int port)
        {
            Client = new Client();
            Client.PacketProcessor.SubscribeReusable<PlayerSpawned, NebulaConnection>(OnPlayerSpawned);
            Client.PacketProcessor.SubscribeReusable<Movement, NebulaConnection>(OnPlayerMovement);

            Client.Connect(ip, port);

            PlayerManager = new PlayerManager();
        }

        public void Disconnect()
        {
            if (Client != null && Client.IsConnected)
            {
                Client.Disconnect();
                Client = null;
            }
        }

        void OnDestroy()
        {
            Disconnect();
        }

        void Update()
        {
            if (Client != null)
            {
                Client.Update();
            }
        }

        private void OnPlayerSpawned(PlayerSpawned packet, NebulaConnection conn)
        {
            // TODO: Spawn player remote model and add it to a dictionnary
            GameMain.mainPlayer.
        }

        private void OnPlayerMovement(Movement packet, NebulaConnection conn)
        {
            // TODO: Move player model
        }
    }
}
