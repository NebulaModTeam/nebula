#region

using System;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Utils;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

#pragma warning disable IDE1006
namespace NebulaWorld.MonoBehaviours.Local.Chat;

/// <summary>
///     This is what is rendered in the chat area (already sent chat messages)
/// </summary>
[Serializable]
public class TMProChatMessage
{
    public TMP_Text textObject;
    public TMP_Text notificationText;
    private ChatMessageType messageType;
    private string text;

    public TMProChatMessage(GameObject textObj, string message, ChatMessageType messageType)
    {
        textObject = textObj.GetComponent<TMP_Text>();
        Text = message;
        MessageType = messageType;
    }


    public string Text
    {
        get => text;
        set
        {
            textObject.text = value;
            if (notificationText != null)
            {
                notificationText.text = value;
            }

            text = value;
        }
    }

    public ChatMessageType MessageType
    {
        get => messageType;
        set
        {
            textObject.color = ChatUtils.GetMessageColor(value);
            if (notificationText != null)
            {
                notificationText.color = textObject.color;
            }

            messageType = value;
        }
    }

    public void DestroyMessage()
    {
        Object.Destroy(textObject.gameObject);
        if (notificationText != null)
        {
            Object.Destroy(notificationText.gameObject);
        }
    }
}
#pragma warning restore IDE1006
