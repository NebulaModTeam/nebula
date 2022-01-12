using CommonAPI.Systems;
using NebulaModel;
using NebulaModel.Logger;
using NebulaModel.Packets.Players;
using NebulaWorld.Chat;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NebulaWorld.MonoBehaviours.Local
{
    public class ChatManager : MonoBehaviour
    {
        public TMP_InputField chatBox;
        public int maxMessages = 25;
        public GameObject chatPanel, textObject, notifier, chatWindow;
        [SerializeField] private List<Message> messages = new List<Message>();
        public Color playerMessage, info;
        private int _attemptsToGetLocationCountDown = 25;
        private bool _sentLocation;
        private Queue<QueuedMessage> _queuedMessages = new Queue<QueuedMessage>(5);

        private void OnEnable()
        {
            Toggle(true);
        }

        void Update()
        {
            if (CustomKeyBindSystem.GetKeyBind("NebulaChatWindow").keyValue)
            {
                Log.Info("Chat window keybind triggered");
                Toggle();
            }

            if (chatBox.text != "")
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    if (Multiplayer.IsActive)
                    {
                        Multiplayer.Session.Network.SendPacket(new NewChatMessagePacket(ChatMessageType.PlayerMessage,
                            chatBox.text, DateTime.Now, GetUserName()));
                        SendMessageToChat($"[{DateTime.Now:HH:mm}] [{GetUserName()}] : {chatBox.text}", ChatMessageType.PlayerMessage);
                    }
                    else
                    {
                        Log.Debug($"Chat message is only sent locally");
                    }

                    chatBox.text = "";
                }
                else
                {
                    if (!chatBox.isFocused && Input.GetKeyDown(KeyCode.Return))
                        chatBox.ActivateInputField();
                }
            }

            SendPlanetInfoMessage();
            if (_queuedMessages.Count > 0)
            {
                QueuedMessage queuedMessage = _queuedMessages.Dequeue();
                SendMessageToChat(queuedMessage.MessageText, queuedMessage.ChatMessageType);
            }
        }

        private static string GetUserName()
        {
            return Multiplayer.Session.LocalPlayer.Data.Username;
        }

        private void SendPlanetInfoMessage()
        {
            if (_sentLocation)
                return;
            if (GameMain.localPlanet == null && _attemptsToGetLocationCountDown-- > 0)
            {
                return;
            }

            string locationStr = GameMain.localPlanet == null ? "In Space" : GameMain.localPlanet.displayName;
            Multiplayer.Session.Network.SendPacket(new NewChatMessagePacket(ChatMessageType.SystemMessage,
                $"Connected, current location {locationStr}", DateTime.Now, GetUserName()));
            _sentLocation = true;
        }

        // Queue a message to appear in chat window
        public void QueueChatMessage(string text, ChatMessageType messageType)
        {
            _queuedMessages.Enqueue(new QueuedMessage { MessageText = text, ChatMessageType = messageType });
        }


        // This one is private, outsiders should call QueueChatMessage
        private void SendMessageToChat(string text, ChatMessageType messageType)
        {
            if (messages.Count > maxMessages)
            {
                Destroy(messages[0].textObject.gameObject);
                messages.Remove(messages[0]);
            }

            var newMsg = new Message { text = text };
            GameObject nextText = Instantiate(textObject, chatPanel.transform);
            newMsg.textObject = nextText.GetComponent<TMP_Text>();
            newMsg.textObject.text = newMsg.text;
            newMsg.textObject.color = MessageTypeColor(messageType);
            Log.Info($"Adding message: {messageType} {newMsg.text}");
            messages.Add(newMsg);
            // alert user of new chat if panel closed
            if (!chatWindow.activeSelf)
            {
                if (Config.Options.AutoOpenChat)
                {
                    Toggle();
                }
                else
                {
                    notifier.SetActive(true);
                }
            }
        }

        private Color MessageTypeColor(ChatMessageType messageType)
        {
            Color color = Color.white;
            switch (messageType)
            {
                case ChatMessageType.PlayerMessage:
                    color = playerMessage;
                    break;
                case ChatMessageType.SystemMessage:
                    color = info;
                    break;
            }

            return color;
        }

        public void Toggle(bool forceClosed = false)
        {
            bool desiredStatus = !forceClosed && !chatWindow.activeSelf;
            chatWindow.SetActive(desiredStatus);
            if (chatWindow.activeSelf)
            {
                // when the window is activated we assume user wants to type right away
                chatBox.ActivateInputField();
                notifier.SetActive(false);
            }
        }

        public void InsertSprite()
        {
            Vector2 pos =  new Vector2(-300, 238);
            UISignalPicker.Popup(pos, singalId =>
            {
                if (singalId <= 0) return;
                
                uint spriteIndex = ChatRichTextManager.signalSpriteIndex[singalId];
                if (spriteIndex >= ChatRichTextManager.iconsSpriteAsset.spriteCharacterTable.Count) return;

                TMP_SpriteCharacter character = ChatRichTextManager.iconsSpriteAsset.spriteCharacterTable[(int)spriteIndex];
                string richText = $"<sprite name=\"{character.name}\">";
                chatBox.text = chatBox.text.Insert(chatBox.stringPosition, richText);
            });
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

    internal class QueuedMessage
    {
        public string MessageText;
        public ChatMessageType ChatMessageType;
    }
}