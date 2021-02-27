using NebulaModel.DataStructures;
using NebulaModel.Packets;
using System;
using System.Collections;
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
            if (MultiplayerSession.instance?.Client.IsSessionJoined ?? false)
            {
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

        IEnumerator SessionTick(float dt, Action callback)
        {
            while (true)
            {
                if (MultiplayerSession.instance?.Client.IsSessionJoined ?? false)
                {
                    callback();
                }

                yield return new WaitForSeconds(dt);
            }
        }
    }
}
