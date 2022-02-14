using NebulaModel.Packets.Players;
using NebulaModel.Utils;
using System;
using TMPro;
using UnityEngine;

namespace NebulaModel.DataStructures
{
    /// <summary>
    /// This is what is rendered in the chat area (already sent chat messages)
    /// </summary>
    [Serializable]
    public class ChatMessage
    {
        private string text;
        private ChatMessageType messageType;

        public TMP_Text textObject;
        public TMP_Text notificationText;


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

        public ChatMessage(GameObject textObj, string message, ChatMessageType messageType)
        {
            textObject = textObj.GetComponent<TMP_Text>();
            Text = message;
            MessageType = messageType;
        }
    }
}