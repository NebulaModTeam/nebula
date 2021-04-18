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

                Double3 uPosition = new Double3(GameMain.mainPlayer.uPosition.x, GameMain.mainPlayer.uPosition.y, GameMain.mainPlayer.uPosition.z);
                LocalPlayer.SendPacket(new PlayerMovement(LocalPlayer.PlayerId, GameMain.localPlanet?.id ?? -1, rootTransform.position.ToFloat3(), uPosition, rotation, bodyRotation));

                LocalPlayer.Data.BodyRotation = bodyRotation;
                LocalPlayer.Data.LocalPlanetId = GameMain.localPlanet?.id ?? -1;
                LocalPlayer.Data.LocalPlanetPosition = rootTransform.position.ToFloat3();
                LocalPlayer.Data.LocalStarId = GameMain.localStar?.id ?? -1;
                LocalPlayer.Data.Rotation = rotation;
                LocalPlayer.Data.UPosition = uPosition;
            }
        }
    }
}
