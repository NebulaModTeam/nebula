using NebulaModel.GameLogic;
using NebulaModel.Packets.Players;
using UnityEngine;

namespace NebulaClient.MonoBehaviours.Local
{
    public class LocalPlayerMovement : MonoBehaviour
    {
        public const int SEND_RATE = 15;
        public const float BROADCAST_INTERVAL = 1f / SEND_RATE;

        private float time;
        private Transform rootTransform;
        private Transform bodyTransform;

        void Awake()
        {
            rootTransform = GetComponent<Transform>();
            bodyTransform = rootTransform.Find("Model");
        }

        void Update()
        {
            time += Time.deltaTime;

            if (time >= BROADCAST_INTERVAL)
            {
                time = 0;

                Vector3 position = rootTransform.position;
                Vector3 rotation = rootTransform.eulerAngles;
                Vector3 bodyRotation = bodyTransform.eulerAngles;

                LocalPlayer.SendPacket(new PlayerMovement(position, rotation, bodyRotation), LiteNetLib.DeliveryMethod.Sequenced);
            }
        }
    }
}
