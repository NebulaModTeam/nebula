#region

using System;
using TMPro;
using UnityEngine;

#endregion

namespace NebulaWorld.MonoBehaviours.Local.Chat;

public class NotificationMessage : MonoBehaviour
{
    private const long NOTIFICATION_DURATION_TICKS = TimeSpan.TicksPerSecond;
    private const long FADE_DURATION = TimeSpan.TicksPerSecond * 2;

    private long notifierEndTime;
    private TMP_Text notifierText;

    private void Update()
    {
        if (notifierEndTime >= DateTime.Now.Ticks)
        {
            return;
        }

        if (notifierEndTime + FADE_DURATION >= DateTime.Now.Ticks)
        {
            float fadeTime = DateTime.Now.Ticks - notifierEndTime;
            notifierText.alpha = 1f - fadeTime / FADE_DURATION;
        }
        else
        {
            // To avoid error, set it inactive here and let TMProChatMessage do the destroy gameObject work
            gameObject.SetActive(false);
        }
    }

    public void Init(string text, Color color, int duration)
    {
        notifierEndTime = DateTime.Now.Ticks + NOTIFICATION_DURATION_TICKS * duration;
        notifierText = GetComponent<TMP_Text>();
        notifierText.text = text;
        notifierText.color = color;
    }
}
