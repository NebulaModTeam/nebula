using NebulaModel.DataStructures;
using NebulaModel.Packets.Players;
using UnityEngine;

namespace NebulaWorld.MonoBehaviours.Local
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

                var rotation = new Float3(rootTransform.eulerAngles);
                var bodyRotation = new Float3(bodyTransform.eulerAngles);

                var position = new Double3();
                if (GameMain.localPlanet != null)
                {
                    position.x = rootTransform.position.x;
                    position.y = rootTransform.position.y;
                    position.z = rootTransform.position.z;
                }
                else
                {
                    position.x = GameMain.mainPlayer.uPosition.x;
                    position.y = GameMain.mainPlayer.uPosition.y;
                    position.z = GameMain.mainPlayer.uPosition.z;
                }

                LocalPlayer.SendPacket(new PlayerMovement(LocalPlayer.PlayerId, GameMain.localPlanet?.id ?? -1, position, rotation, bodyRotation));
            }
        }
    }
}
