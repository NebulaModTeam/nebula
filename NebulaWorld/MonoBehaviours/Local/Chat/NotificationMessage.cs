// unset

using System;
using TMPro;
using UnityEngine;

namespace NebulaWorld.MonoBehaviours.Local
{
    public class NotificationMessage : MonoBehaviour
    {
        private const long NOTIFICATION_DURATION_TICKS = TimeSpan.TicksPerSecond;
        private const long FADE_DURATION = TimeSpan.TicksPerSecond * 2;
        
        public long notifierEndTime;
        public TMP_Text text;

        public void Init(int duration)
        {
            notifierEndTime = DateTime.Now.Ticks + NOTIFICATION_DURATION_TICKS * duration;
            text = GetComponent<TMP_Text>();
        }

        private void Update()
        {
            if (notifierEndTime >= DateTime.Now.Ticks) return;
            
            if (notifierEndTime + FADE_DURATION >= DateTime.Now.Ticks)
            {
                float fadeTime = DateTime.Now.Ticks - notifierEndTime;
                text.alpha = 1f - fadeTime / FADE_DURATION;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}