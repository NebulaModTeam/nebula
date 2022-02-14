using NebulaModel;
using NebulaModel.DataStructures;
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
        
        [SerializeField] private TMP_InputField chatBox;
        [SerializeField] private RectTransform chatPanel;
        [SerializeField] private GameObject textObject;
        [SerializeField] private RectTransform notifier;
        [SerializeField] private RectTransform notifierMask;
        
        [SerializeField] private GameObject chatWindow;
        
        
        internal UIWindowDrag DragTrigger;
        private Queue<QueuedMessage> outgoingMessages = new Queue<QueuedMessage>(5);
        private readonly List<ChatMessage> messages = new List<ChatMessage>();
        
        public string userName;

        private void Awake()
        {
            DragTrigger = GetComponent<UIWindowDrag>();
        }

        void Update()
        {
            if (!chatWindow.activeSelf) return;

            notifierMask.sizeDelta = new Vector2(chatPanel.rect.width, notifierMask.sizeDelta.y);

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
            if (!chatBox.text.Equals(""))
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

        public ChatMessage SendLocalChatMessage(string text, ChatMessageType messageType)
        {
            text = ChatUtils.SanitizeText(text);
            if (messages.Count > MAX_MESSAGES)
            {
                messages[0].DestroyMessage();
                messages.Remove(messages[0]);
            }

            
            GameObject textObj = Instantiate(textObject, chatPanel);
            ChatMessage newMsg = new ChatMessage(textObj, text, messageType);

            GameObject notificationMsg = Instantiate(textObj, notifier);
            newMsg.notificationText = notificationMsg.GetComponent<TMP_Text>();
            NotificationMessage message = notificationMsg.AddComponent<NotificationMessage>();
            message.Init();
            
            messages.Add(newMsg);

            if (!chatWindow.activeSelf)
            {
                if (Config.Options.AutoOpenChat)
                {
                    Toggle();
                }
            }

            return newMsg;
        }

        public void ClearChat()
        {
            foreach (ChatMessage message in messages)
            {
                message.DestroyMessage();
            }
            messages.Clear();
        }
        

        public void Toggle(bool forceClosed = false)
        {
            bool desiredStatus = !forceClosed && !chatWindow.activeSelf;
            chatWindow.SetActive(desiredStatus);
            notifier.gameObject.SetActive(!desiredStatus);
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
        
        public void InsertEmoji()
        {
            EmojiPicker.Open(emoji =>
            {
                chatBox.Insert($"<sprite name=\"{emoji.UnifiedCode.ToLower()}\">");
                FocusInputField();
            });
        }
        
        public void InsertSprite()
        {
            Vector2 pos =  new Vector2(-300, 238);
            UISignalPicker.Popup(pos, signalId =>
            {
                if (signalId <= 0) return;

                string richText = RichChatLinkRegistry.FormatShortRichText(SignalChatLinkHandler.GetLinkString(signalId));
                chatBox.Insert(richText);
                FocusInputField();
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
}