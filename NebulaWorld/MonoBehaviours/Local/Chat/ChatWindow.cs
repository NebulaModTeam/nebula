using NebulaModel.Packets.Players;
using NebulaModel.Utils;
using NebulaWorld.MonoBehaviours.Local;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NebulaWorld.Chat
{
    public class ChatWindow : MonoBehaviour
    {
        private const int MAX_MESSAGES = 200;
        
        
        public TMP_InputField chatBox;
        public GameObject chatPanel, textObject, notifier, chatWindow;

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
                    BroadcastMessage(formattedMessage);

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

        private void BroadcastMessage(string message)
        {
            QueueOutgoingMessage(message, 0);
            SendLocalMessage(message, 0);
        }

        private void QueueOutgoingMessage(string message, ChatMessageType chatMesageType)
        {
            outgoingMessages.Enqueue(new QueuedMessage { MessageText = message, ChatMessageType = chatMesageType });
        }

        public void SendLocalMessage(string text, ChatMessageType messageType)
        {
            text = ChatUtils.SanitizeText(text);
            if (messages.Count > MAX_MESSAGES)
            {
                Destroy(messages[0].textObject.gameObject);
                messages.Remove(messages[0]);
            }

            var newMsg = new Message { text = text };
            GameObject nextText = Instantiate(textObject, chatPanel.transform);
            newMsg.textObject = nextText.GetComponent<TMP_Text>();
            newMsg.textObject.text = newMsg.text;
            newMsg.textObject.color = ChatUtils.GetMessageColor(messageType);
            
            GameObject notificationMsg = Instantiate(nextText, notifier.transform);
            NotificationMessage message = notificationMsg.AddComponent<NotificationMessage>();
            message.Init();
            
            Console.WriteLine($"Adding message: {messageType} {newMsg.text}");
            messages.Add(newMsg);
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
            else
            {
                ChatLinkTrigger.CloseTips();
            }
        }
        
        public void InsertSprite()
        {
            Vector2 pos =  new Vector2(-300, 238);
            UISignalPicker.Popup(pos, signalId =>
            {
                if (signalId <= 0) return;

                string richText = RichChatLinkRegistry.FormatShortRichText(SignalChatLinkHandler.GetLinkString(signalId));
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
            public ChatMessageType ChatMessageType;
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
        public ChatMessageType messageType;
    }
}