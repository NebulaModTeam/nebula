using NebulaModel.Networking;
using NebulaModel.Packets;
using UnityEngine;

namespace NebulaClient.MonoBehaviours
{
    public class MultiplayerSession : MonoBehaviour
    {
        public static MultiplayerSession instance;

        public Client Client { get; private set; }
        public RemotePlayerManager RemotePlayerManager { get; private set; }


        void Awake()
        {
            instance = this;

            Client = new Client();
            Client.PacketProcessor.SubscribeReusable<Movement>(OnPlayerMovement);
            Client.PacketProcessor.SubscribeReusable<PlayerAnimationUpdate>(OnPlayerAnimationUpdate);
            Client.PacketProcessor.SubscribeReusable<RemotePlayerJoined>(OnRemotePlayerJoined);
            Client.PacketProcessor.SubscribeReusable<PlayerDisconnected>(OnRemotePlayerDisconnect);
        }

        public void Connect(string ip, int port)
        {
            Client.Connect(ip, port);

            RemotePlayerManager = new RemotePlayerManager();
        }

        public void Disconnect()
        {
            if (Client.IsConnected)
            {
                Client.Disconnect();
            }

            CleanupSession();
        }

        void OnDestroy()
        {
            Disconnect();
        }

        void CleanupSession()
        {
            // TODO: remove remote player models from the scene
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
    }
}
