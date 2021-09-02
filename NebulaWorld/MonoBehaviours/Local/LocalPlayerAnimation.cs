using NebulaModel.DataStructures;
using NebulaModel.Packets.Players;
using UnityEngine;

namespace NebulaWorld.MonoBehaviours.Local
{
    public class LocalPlayerAnimation : MonoBehaviour
    {
        public const int SEND_RATE = 10;
        public const float BROADCAST_INTERVAL = 1f / SEND_RATE;

        private float time;
        private PlayerAnimator playerAnimator;

        void Awake()
        {
            playerAnimator = GetComponent<PlayerAnimator>();
        }

        void Update()
        {
            time += Time.deltaTime;

            if (time >= BROADCAST_INTERVAL)
            {
                time = 0;

                PlayerController controller = playerAnimator.player.controller;
                float vertSpeed = Vector3.Dot(base.transform.up, controller.velocity);
                Vector3 horzVel = controller.velocity - vertSpeed * base.transform.up;
                float horzSpeed = horzVel.magnitude;

                Multiplayer.Session.Network.SendPacket(new PlayerAnimationUpdate()
                {
                    PlayerId = Multiplayer.Session.LocalPlayer.Id,
                    Idle = playerAnimator.idle.ToNebula(),
                    RunSlow = playerAnimator.runSlow.ToNebula(),
                    RunFast = playerAnimator.runFast.ToNebula(),
                    Drift = playerAnimator.drift.ToNebula(),
                    DriftF = playerAnimator.driftF.ToNebula(),
                    DriftL = playerAnimator.driftL.ToNebula(),
                    DriftR = playerAnimator.driftR.ToNebula(),
                    Fly = playerAnimator.fly.ToNebula(),
                    Sail = playerAnimator.sail.ToNebula(),
                    Mining0 = playerAnimator.mining0.ToNebula(),
                    horzSpeed = horzSpeed,
                    vertSpeed = vertSpeed,
                });
            }
        }
    }
}
