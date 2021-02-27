using NebulaModel.DataStructures;
using NebulaModel.Packets;
using UnityEngine;

namespace NebulaClient.MonoBehaviours
{
    public class RemotePlayerAnimator : MonoBehaviour
    {
        private Animation anim;

        public AnimationState idle { get; private set; }
        public AnimationState runSlow { get; private set; }
        public AnimationState runFast { get; private set; }
        public AnimationState drift { get; private set; }
        public AnimationState driftF { get; private set; }
        public AnimationState driftL { get; private set; }
        public AnimationState driftR { get; private set; }
        public AnimationState fly { get; private set; }
        public AnimationState sail { get; private set; }
        public AnimationState mining0 { get; private set; }

        public void Awake()
        {
            anim = GetComponentInChildren<Animation>();

            idle = anim["idle"];
            runSlow = anim["run-slow"];
            runFast = anim["run-fast"];
            drift = anim["drift"];
            driftF = anim["drift-f"];
            driftL = anim["drift-l"];
            driftR = anim["drift-r"];
            fly = anim["fly"];
            sail = anim["sail"];
            mining0 = anim["mining-0"];

            idle.layer = 0;
            runSlow.layer = 1;
            runFast.layer = 1;
            drift.layer = 2;
            driftF.layer = 2;
            driftL.layer = 2;
            driftR.layer = 2;
            fly.layer = 3;
            sail.layer = 3;
            mining0.layer = 4;
            idle.weight = 1f;
            runSlow.weight = 0.0f;
            runFast.weight = 0.0f;
            drift.weight = 0.0f;
            fly.weight = 0.0f;
            sail.weight = 0.0f;
            mining0.weight = 0.0f;
            mining0.speed = 0.8f;
        }

        public void UpdateState(PlayerAnimationUpdate packet)
        {
            ApplyAnimationState(idle, packet.Idle);
            ApplyAnimationState(runSlow, packet.RunSlow);
            ApplyAnimationState(runFast, packet.RunFast);
            ApplyAnimationState(drift, packet.Drift);
            ApplyAnimationState(driftF, packet.DriftF);
            ApplyAnimationState(driftL, packet.DriftL);
            ApplyAnimationState(driftR, packet.DriftR);
            ApplyAnimationState(fly, packet.Fly);
            ApplyAnimationState(sail, packet.Sail);
            ApplyAnimationState(mining0, packet.Mining0);
        }

        private void ApplyAnimationState(AnimationState animState, NebulaAnimationState newState)
        {
            animState.weight = newState.Weight;
            animState.speed = newState.Speed;
            animState.enabled = newState.Enabled;
        }
    }
}
