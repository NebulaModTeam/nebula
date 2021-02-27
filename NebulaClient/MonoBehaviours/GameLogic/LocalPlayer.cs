using NebulaModel.DataStructures;
using NebulaModel.Packets;
using UnityEngine;

namespace NebulaClient.MonoBehaviours.GameLogic
{
    public class LocalPlayer : MonoBehaviour
    {
        private Transform playerTransform;
        private Transform playerModelTransform;
        private PlayerAnimator anim;

        void Start()
        {
            playerTransform = GetComponent<Transform>();
            playerModelTransform = playerTransform.Find("Model");
            anim = GetComponent<PlayerAnimator>();
        }

        void Update()
        {
            // TODO: We should only send update 20 times per seconds and smooth the position / animation on the clients.
            if (MultiplayerSession.instance?.Client.IsSessionJoined ?? false)
            {
                // TODO: Investigate if we can reuse the Movement and PlayerAnimationUpdate packets to avoid GC here.

                // Send Transform Update
                MultiplayerSession.instance.Client.SendPacket(new Movement(playerTransform, playerModelTransform), LiteNetLib.DeliveryMethod.Unreliable);

                // Send Animation Update
                MultiplayerSession.instance.Client.SendPacket(new PlayerAnimationUpdate()
                {
                    Idle = anim.idle.ToNebula(),
                    RunSlow = anim.runSlow.ToNebula(),
                    RunFast = anim.runFast.ToNebula(),
                    Drift = anim.drift.ToNebula(),
                    DriftF = anim.driftF.ToNebula(),
                    DriftL = anim.driftL.ToNebula(),
                    DriftR = anim.driftR.ToNebula(),
                    Fly = anim.fly.ToNebula(),
                    Sail = anim.sail.ToNebula(),
                    Mining0 = anim.mining0.ToNebula(),
                }, LiteNetLib.DeliveryMethod.Unreliable);
            }
        }
    }
}
