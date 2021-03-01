using NebulaModel.Packets.Players;
using UnityEngine;

namespace NebulaClient.MonoBehaviours.Local
{
    public class LocalPlayerMovement : MonoBehaviour
    {
        public const float BROADCAST_INTERVAL = 0.05f; // 20 updates per seconds

        private float time;
        private Transform rootTransform;
        private Transform bodyTransform;

        void Start()
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

                MultiplayerSession.instance.Client?.SendPacket(new Movement(position, rotation, bodyRotation), LiteNetLib.DeliveryMethod.Sequenced);
            }
        }
    }
}
