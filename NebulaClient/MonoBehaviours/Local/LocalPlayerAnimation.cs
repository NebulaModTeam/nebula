using NebulaModel.DataStructures;
using NebulaModel.Packets;
using UnityEngine;

namespace NebulaClient.MonoBehaviours.Local
{
    public class LocalPlayerAnimation : MonoBehaviour
    {
        public const float BROADCAST_INTERVAL = 0.2f; // 5 updates per seconds

        private float time;
        private PlayerAnimator playerAnimator;

        void Start()
        {
            playerAnimator = GetComponent<PlayerAnimator>();
        }

        void Update()
        {
            time += Time.deltaTime;

            if (time >= BROADCAST_INTERVAL)
            {
                time = 0;

                MultiplayerSession.instance.Client?.SendPacket(new PlayerAnimationUpdate()
                {
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
                }, LiteNetLib.DeliveryMethod.Unreliable);
            }
        }
    }
}
