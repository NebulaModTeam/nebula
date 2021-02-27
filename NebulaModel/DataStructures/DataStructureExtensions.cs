using UnityEngine;

namespace NebulaModel.DataStructures
{
    public static class DataStructureExtensions
    {
        public static Vector3 ToUnity(this Float3 value)
        {
            return new Vector3(value.x, value.y, value.z);
        }

        public static Float3 ToNebula(this Vector3 value)
        {
            return new Float3(value.x, value.y, value.z);
        }

        public static Quaternion ToUnity(this Float4 value)
        {
            return new Quaternion(value.x, value.y, value.z, value.w);
        }

        public static Float4 ToNebula(this Quaternion value)
        {
            return new Float4(value.x, value.y, value.z, value.w);
        }

        public static NebulaTransform ToNebula(this Transform transform)
        {
            return new NebulaTransform()
            {
                Position = transform.position.ToNebula(),
                Rotation = transform.eulerAngles.ToNebula(),
                Scale = transform.localScale.ToNebula(),
            };
        }

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
