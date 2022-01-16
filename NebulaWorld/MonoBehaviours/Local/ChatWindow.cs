using NebulaModel.Logger;
using NebulaModel.Utils;
using NebulaWorld.Chat;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace NebulaWorld.MonoBehaviours.Local
{
    public class ChatWindow : MonoBehaviour
    {
        private const int MAX_MESSAGES = 200;
        
        
        public TMP_InputField chatBox;
        public GameObject chatPanel, textObject, notifier, chatWindow;
        public Color playerMessage, info;
        
        private Queue<QueuedMessage> outgoingMessages = new Queue<QueuedMessage>(5);
        private readonly List<Message> messages = new List<Message>();
        
        public string userName;

        void Update()
        {
            if (!chatWindow.activeSelf) return;
            
            if (chatBox.text != "")
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    string formattedMessage = $"[{DateTime.Now:HH:mm}] [{userName}] : {chatBox.text}";
                    
                    QueueOutgoingMessage(formattedMessage, 0);
                    SendMessageToChat(formattedMessage, 0);

                    chatBox.text = "";
                    // bring cursor back to message area so they can keep typing
                    chatBox.ActivateInputField();
                }
                else
                {
                    if (!chatBox.isFocused && Input.GetKeyDown(KeyCode.Return))
                        chatBox.ActivateInputField();
                }
            }

            if (VFInput.escKey.onDown || VFInput.escape)
            {
                Toggle(true);
                VFInput.UseEscape();
            }
        }

        private void QueueOutgoingMessage(string message, int chatMesageType)
        {
            outgoingMessages.Enqueue(new QueuedMessage { MessageText = message, ChatMessageType = chatMesageType });
        }

        public void SendMessageToChat(string text, int messageType)
        {
            text = TextUtils.SanitizeText(text);
            if (messages.Count > MAX_MESSAGES)
            {
                Destroy(messages[0].textObject.gameObject);
                messages.Remove(messages[0]);
            }

            var newMsg = new Message { text = text };
            GameObject nextText = Instantiate(textObject, chatPanel.transform);
            newMsg.textObject = nextText.GetComponent<TMP_Text>();
            newMsg.textObject.text = newMsg.text;
            newMsg.textObject.color = MessageTypeColor(messageType);
            
            GameObject notificationMsg = Instantiate(nextText, notifier.transform);
            NotificationMessage message = notificationMsg.AddComponent<NotificationMessage>();
            message.Init();
            
            Console.WriteLine($"Adding message: {messageType} {newMsg.text}");
            messages.Add(newMsg);
        }


        private Color MessageTypeColor(int messageType)
        {
            Color color = Color.white;
            switch (messageType)
            {
                case 0:
                    color = playerMessage;
                    break;
                case 1:
                    color = info;
                    break;
                default:
                    Console.WriteLine($"Requested color for unexpected chat message type {messageType}");
                    break;
            }

            return color;
        }

        public void Toggle(bool forceClosed = false)
        {
            bool desiredStatus = !forceClosed && !chatWindow.activeSelf;
            chatWindow.SetActive(desiredStatus);
            notifier.SetActive(!desiredStatus);
            if (chatWindow.activeSelf)
            {
                // when the window is activated we assume user wants to type right away
                chatBox.ActivateInputField();
            }
        }
        
        public void InsertSprite()
        {
            Vector2 pos =  new Vector2(-300, 238);
            UISignalPicker.Popup(pos, singalId =>
            {
                if (singalId <= 0) return;
                
                string richText = $"<sprite name=\"{singalId}\">";
                chatBox.Insert(richText);
                chatBox.ActivateInputField();
            });
        }

        public QueuedMessage GetQueuedMessage()
        {
            return outgoingMessages.Count > 0 ? outgoingMessages.Dequeue() : null;
        }

        public class QueuedMessage
        {
            public string MessageText;
            public int ChatMessageType;
        }
    }


    /// <summary>
    /// This is what is rendered in the chat area (already sent chat messages)
    /// </summary>
    [Serializable]
    public class Message
    {
        public string text;
        public TMP_Text textObject;
        public int messageType;
    }
}