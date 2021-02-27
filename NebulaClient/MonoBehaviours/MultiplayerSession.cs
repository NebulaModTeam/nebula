using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets;
using UnityEngine;

namespace NebulaClient.MonoBehaviours
{
    public class MultiplayerSession : MonoBehaviour
    {
        public static MultiplayerSession instance;

        public Client Client { get; private set; }
        public PlayerManager PlayerManager { get; private set; }


        void Awake()
        {
            instance = this;

            Client = new Client();
            Client.PacketProcessor.SubscribeReusable<Movement, NebulaConnection>(OnPlayerMovement);
            Client.PacketProcessor.SubscribeReusable<PlayerAnimationUpdate, NebulaConnection>(OnPlayerAnimationUpdate);
        }

        public void Connect(string ip, int port)
        {
            Client.Connect(ip, port);

            PlayerManager = new PlayerManager();
        }

        public void Disconnect()
        {
            if (Client.IsConnected)
            {
                Client.Disconnect();
            }
        }

        void OnDestroy()
        {
            Disconnect();
        }

        void Update()
        {
            Client.Update();
        }

        private void OnPlayerMovement(Movement packet, NebulaConnection conn)
        {
            if (PlayerManager.GetPlayerById(conn.Id) == null)
            {
                PlayerManager.AddRemotePlayer(conn.Id, packet);
            }

            var player = PlayerManager.GetPlayerById(conn.Id);
            if (player != null)
            {
                player.PlayerTransform.position = packet.Transform.Position.ToUnity();
                player.PlayerTransform.eulerAngles = packet.Transform.Rotation.ToUnity();
                player.PlayerTransform.localScale = packet.Transform.Scale.ToUnity();
                player.PlayerModelTransform.position = packet.ModelTransform.Position.ToUnity();
                player.PlayerModelTransform.eulerAngles = packet.ModelTransform.Rotation.ToUnity();
                player.PlayerModelTransform.localScale = packet.ModelTransform.Scale.ToUnity();
            }
        }

        private void OnPlayerAnimationUpdate(PlayerAnimationUpdate packet, NebulaConnection conn)
        {
            var player = PlayerManager.GetPlayerById(conn.Id);
            if (player != null)
            {
                player.Animator.UpdateState(packet);
            }
        }
    }
}
