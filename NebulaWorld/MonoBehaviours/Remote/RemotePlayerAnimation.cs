using NebulaModel.Packets.Players;
using UnityEngine;

namespace NebulaWorld.MonoBehaviours.Remote
{
    // TODO: Missing client side interpolation
    public class RemotePlayerAnimation : MonoBehaviour
    {
        private PlayerAnimator playerAnimator;

        private void Awake()
        {
            playerAnimator = GetComponentInChildren<PlayerAnimator>();
        }

        public void UpdateState(PlayerAnimationUpdate packet)
        {
            if (playerAnimator == null)
                return;

            playerAnimator.runWeight = packet.RunWeight;
            playerAnimator.driftWeight = packet.DriftWeight;
            playerAnimator.flyWeight = packet.FlyWeight;
            playerAnimator.sailWeight = packet.SailWeight;
            playerAnimator.jumpWeight = packet.JumpWeight;
            playerAnimator.jumpNormalizedTime = packet.JumpNormalizedTime;
            playerAnimator.idleAnimIndex = packet.IdleAnimIndex;
            playerAnimator.sailAnimIndex = packet.SailAnimIndex;
            playerAnimator.miningWeight = packet.MiningWeight;
            playerAnimator.miningAnimIndex = packet.MiningAnimIndex;
            playerAnimator.sailAnimWeights = packet.SailAnimWeights;
        }
    }
}
