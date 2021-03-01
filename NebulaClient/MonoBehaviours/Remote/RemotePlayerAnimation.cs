using NebulaModel.DataStructures;
using NebulaModel.Packets;
using UnityEngine;

namespace NebulaClient.MonoBehaviours.Remote
{
    // TODO: Missing client side interpolation
    public class RemotePlayerAnimation : MonoBehaviour
    {
        private Animation anim;

        public AnimationState Idle { get; private set; }
        public AnimationState RunSlow { get; private set; }
        public AnimationState RunFast { get; private set; }
        public AnimationState Drift { get; private set; }
        public AnimationState DriftF { get; private set; }
        public AnimationState DriftL { get; private set; }
        public AnimationState DriftR { get; private set; }
        public AnimationState Fly { get; private set; }
        public AnimationState Sail { get; private set; }
        public AnimationState Mining0 { get; private set; }

        public void Awake()
        {
            anim = GetComponentInChildren<Animation>();

            Idle = anim["idle"];
            RunSlow = anim["run-slow"];
            RunFast = anim["run-fast"];
            Drift = anim["drift"];
            DriftF = anim["drift-f"];
            DriftL = anim["drift-l"];
            DriftR = anim["drift-r"];
            Fly = anim["fly"];
            Sail = anim["sail"];
            Mining0 = anim["mining-0"];

            Idle.layer = 0;
            RunSlow.layer = 1;
            RunFast.layer = 1;
            Drift.layer = 2;
            DriftF.layer = 2;
            DriftL.layer = 2;
            DriftR.layer = 2;
            Fly.layer = 3;
            Sail.layer = 3;
            Mining0.layer = 4;
            Idle.weight = 1f;
            RunSlow.weight = 0.0f;
            RunFast.weight = 0.0f;
            Drift.weight = 0.0f;
            Fly.weight = 0.0f;
            Sail.weight = 0.0f;
            Mining0.weight = 0.0f;
            Mining0.speed = 0.8f;
        }

        public void UpdateState(PlayerAnimationUpdate packet)
        {
            ApplyAnimationState(Idle, packet.Idle);
            ApplyAnimationState(RunSlow, packet.RunSlow);
            ApplyAnimationState(RunFast, packet.RunFast);
            ApplyAnimationState(Drift, packet.Drift);
            ApplyAnimationState(DriftF, packet.DriftF);
            ApplyAnimationState(DriftL, packet.DriftL);
            ApplyAnimationState(DriftR, packet.DriftR);
            ApplyAnimationState(Fly, packet.Fly);
            ApplyAnimationState(Sail, packet.Sail);
            ApplyAnimationState(Mining0, packet.Mining0);
        }

        private void ApplyAnimationState(AnimationState animState, NebulaAnimationState newState)
        {
            animState.weight = newState.Weight;
            animState.speed = newState.Speed;
            animState.enabled = newState.Enabled;
        }
    }
}
