using NebulaAPI;
using NebulaModel.Packets.Players;
using UnityEngine;

namespace NebulaWorld.MonoBehaviours.Local
{
    public class LocalPlayerMovement : MonoBehaviour
    {
        public const int SEND_RATE = 10;
        public const float BROADCAST_INTERVAL = 1f / SEND_RATE;

        private float time;
        private Transform rootTransform;
        private Transform bodyTransform;
        private PlayerAnimator playerAnimator;

        private void Awake()
        {
            rootTransform = GetComponent<Transform>();
            bodyTransform = rootTransform.Find("Model");
            playerAnimator = GetComponent<PlayerAnimator>();
        }

        private void Update()
        {
            time += Time.deltaTime;

            if (time >= BROADCAST_INTERVAL)
            {
                time = 0;

                Float3 rotation = new Float3(rootTransform.eulerAngles);
                Float3 bodyRotation = new Float3(bodyTransform.eulerAngles);

                Double3 uPosition = new Double3(GameMain.mainPlayer.uPosition.x, GameMain.mainPlayer.uPosition.y, GameMain.mainPlayer.uPosition.z);
                Multiplayer.Session.Network.SendPacket(new PlayerMovement(Multiplayer.Session.LocalPlayer.Id, GameMain.localPlanet?.id ?? -1, rootTransform.position.ToFloat3(), uPosition, rotation, bodyRotation, playerAnimator));

                IPlayerData playerData = Multiplayer.Session.LocalPlayer.Data;
                playerData.BodyRotation = bodyRotation;
                playerData.LocalPlanetId = GameMain.localPlanet?.id ?? -1;
                playerData.LocalPlanetPosition = rootTransform.position.ToFloat3();
                playerData.LocalStarId = GameMain.localStar?.id ?? -1;
                playerData.Rotation = rotation;
                playerData.UPosition = uPosition;
            }
        }
    }
}
