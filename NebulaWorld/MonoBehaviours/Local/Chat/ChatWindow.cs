using NebulaModel.Packets.Players;
using NebulaModel.Utils;
using NebulaWorld.Chat;
using NebulaWorld.Chat.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace NebulaWorld.MonoBehaviours.Local
{
    public class ChatWindow : MonoBehaviour
    {
        private const int MAX_MESSAGES = 200;
        
        
        public TMP_InputField chatBox;
        public GameObject chatPanel, textObject, notifier, chatWindow;
        public UIWindowDrag dragTrigger;

        private Queue<QueuedMessage> outgoingMessages = new Queue<QueuedMessage>(5);
        private readonly List<Message> messages = new List<Message>();
        
        public string userName;

        private void Awake()
        {
            dragTrigger = GetComponent<UIWindowDrag>();
        }

        void Update()
        {
            if (!chatWindow.activeSelf) return;

            if (chatBox.text != "")
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    TrySendMessage();
                }
                else
                {
                    if (!chatBox.isFocused && Input.GetKeyDown(KeyCode.Return))
                    {
                        FocusInputField();
                    }
                }
            }

            if (VFInput.escKey.onDown || VFInput.escape)
            {
                if (UISignalPicker.isOpened)
                {
                    UISignalPicker.Close();
                    VFInput.UseEscape();
                    return;
                }
                
                if (EmojiPicker.IsOpen())
                {
                    EmojiPicker.Close();
                    VFInput.UseEscape();
                    return;
                }
                
                Toggle(true);
                VFInput.UseEscape();
            }
        }

        private void FocusInputField()
        {
            chatBox.ActivateInputField();
            chatBox.MoveToEndOfLine(false, true);
        }

        private void TrySendMessage()
        {
            if (chatBox.text.StartsWith(ChatCommandRegistry.CommandPrefix))
            {
                string[] arguments = chatBox.text.Substring(1).Split(' ');
                if (arguments.Length > 0)
                {
                    string commandName = arguments[0];
                    IChatCommandHandler handler = ChatCommandRegistry.GetCommandHandler(commandName);
                    if (handler != null)
                    {
                        handler.Execute(this, arguments.Skip(1).ToArray());
                    }
                    else
                    {
                        SendLocalChatMessage($"Unknown command {commandName}. Use /help to get list of commands", ChatMessageType.CommandUsageMessage);
                    }
                }
            }
            else
            {
                BroadcastChatMessage(chatBox.text);
            }
            chatBox.text = "";
            // bring cursor back to message area so they can keep typing
            FocusInputField();
        }

        private void BroadcastChatMessage(string message, ChatMessageType chatMessageType = ChatMessageType.PlayerMessage)
        {
            QueueOutgoingChatMessage(message, chatMessageType);
            string formattedMessage = $"[{DateTime.Now:HH:mm}] [{userName}] : {message}";
            SendLocalChatMessage(formattedMessage, chatMessageType);
        }

        private void QueueOutgoingChatMessage(string message, ChatMessageType chatMesageType)
        {
            outgoingMessages.Enqueue(new QueuedMessage { MessageText = message, ChatMessageType = chatMesageType });
        }

        public void SendLocalChatMessage(string text, ChatMessageType messageType)
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
                FocusInputField();
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