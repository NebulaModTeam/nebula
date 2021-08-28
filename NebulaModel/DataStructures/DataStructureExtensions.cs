using UnityEngine;

namespace NebulaModel.DataStructures
{
    public static class AnimationStateExtensions
    {
        public static NebulaAnimationState ToNebula(this AnimationState state)
        {
            return new NebulaAnimationState()
            {
                Enabled = state.enabled,
                Weight = state.weight,
                Speed = state.speed,
            };
        }
    }
}
