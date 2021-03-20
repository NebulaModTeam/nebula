using System;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaModel.Utils
{
    public class UnityDispatchQueue : MonoBehaviour
    {
        private static UnityDispatchQueue _instance;
        private static UnityDispatchQueue GetInstance() {
            if (!_instance)
            {
                _instance = FindObjectOfType<UnityDispatchQueue>();
            }

            if (!_instance)
            {
                GameObject go = new GameObject(nameof(UnityDispatchQueue));
                _instance = go.AddComponent<UnityDispatchQueue>();
                DontDestroyOnLoad(_instance);
            }

            return _instance;
        }

        private Queue<Action> actionsQueue = new Queue<Action>();

        public static void RunOnMainThread(Action action)
        {
            UnityDispatchQueue instance = GetInstance();
            lock (instance.actionsQueue)
            {
                instance.actionsQueue.Enqueue(action);
            }
        }

        private void Update()
        {
            lock (actionsQueue)
            {
                while(actionsQueue.Count > 0)
                {
                    actionsQueue.Dequeue().Invoke();
                }
            }
        }

        private void OnDestroy()
        {
            _instance = null;
        }
    }
}
