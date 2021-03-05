using UnityEngine;

namespace NebulaPatcher.MonoBehaviours
{
    public static class MonoBehaviourExtension
    {
        public static T AddComponentIfMissing<T>(this GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
            }
            return component;
        }
    }
}
