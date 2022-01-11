using CommonAPI.Systems;
using NebulaModel;
using NebulaModel.Logger;
using NebulaModel.Packets.Players;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NebulaWorld.MonoBehaviours.Local
{
    public class ChatManager : MonoBehaviour
    {
        public InputField chatBox;
        public int maxMessages = 25;
        public GameObject chatPanel, textObject, notifier, chatWindow;
        [SerializeField] private List<Message> messages = new List<Message>();
        public Color playerMessage, info;
        private int _attemptsToGetLocationCountDown = 25;
        private bool _sentLocation;

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

        public void SendMessageToChat(string text, ChatMessageType messageType)
        {
            if (messages.Count > maxMessages)
            {
                Destroy(messages[0].textObject.gameObject);
                messages.Remove(messages[0]);
            }

            var newMsg = new Message { text = text };
            GameObject nextText = Instantiate(textObject, chatPanel.transform);
            newMsg.textObject = nextText.GetComponent<Text>();
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
            bool desiredStatus = forceClosed ? false : !chatWindow.activeSelf;
            chatWindow.SetActive(desiredStatus);
            if (chatWindow.activeSelf)
            {
                // when the window is activated we assume user wants to type right away
                chatBox.ActivateInputField();
                notifier.SetActive(false);
            }
        }
    }

    /// <summary>
    /// This is what is rendered in the chat area (already sent chat messages)
    /// </summary>
    [Serializable]
    public class Message
    {
        public string text;
        public Text textObject;
        public ChatMessageType messageType;
    }
}