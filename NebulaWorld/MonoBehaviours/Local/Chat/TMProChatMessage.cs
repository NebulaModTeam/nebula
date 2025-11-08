#region

using System;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Utils;
using NebulaWorld.Chat;
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
    public TMP_Text chatText;
    public RawChatMessage rawChatMessage;
    public GameObject notificationObj;

    public TMProChatMessage(GameObject chatTextObj, RawChatMessage rawChatMessage)
    {
        chatText = chatTextObj.GetComponent<TMP_Text>();
        SetMessage(rawChatMessage);
    }

    public void SetMessage(RawChatMessage rawChatMessage)
    {
        this.rawChatMessage = rawChatMessage;
        var formattedText = ChatUtils.FormatMessage(rawChatMessage);

        // Apply sanitization for player messages
        if (rawChatMessage.MessageType.IsPlayerMessage())
        {
            formattedText = ChatUtils.SanitizeText(formattedText);
        }
        // Expand rich text tags
        formattedText = RichChatLinkRegistry.ExpandRichTextTags(formattedText);

        SetText(formattedText, rawChatMessage.MessageType);
    }

    public void SetText(string text, ChatMessageType messageType)
    {
        chatText.text = text;
        chatText.color = ChatUtils.GetMessageColor(messageType);
    }

    public void DestroyMessage()
    {
        Object.Destroy(chatText.gameObject);
        Object.Destroy(notificationObj);
    }
}
#pragma warning restore IDE1006
