using UnityEngine;

namespace NebulaClient.Extensions
{
    public static class MonoBehaviourExtensions
    {
        public static T AddComponentIfMissing<T>(this GameObject behaviour) where T : Component
        {
            T component = behaviour.GetComponent<T>();
            if (!component)
            {
                component = behaviour.AddComponent<T>();
            }
            return component;
        }
    }
}
