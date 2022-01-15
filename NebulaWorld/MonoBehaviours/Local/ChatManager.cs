using CommonAPI.Systems;
using NebulaModel;
using NebulaModel.Logger;
using NebulaModel.Packets.Players;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaWorld.MonoBehaviours.Local
{
    public class ChatManager : MonoBehaviour
    {
        private int attemptsToGetLocationCountDown = 25;
        private bool sentLocation;
        private ChatWindow chatWindow;
        public static ChatManager Instance;

        private void Awake()
        {
            Instance = this;
            GameObject prefab = AssetLoader.AssetBundle.LoadAsset<GameObject>("Assets/Prefab/ChatV2.prefab");
            var uiGameInventory = UIRoot.instance.uiGame.inventory;
            var chatGo = Instantiate(prefab, uiGameInventory.transform.parent, false);
            chatWindow = chatGo.transform.GetComponentInChildren<ChatWindow>();
            chatWindow.userName = GetUserName();
        }

        void Update()
        {
            if (CustomKeyBindSystem.GetKeyBind("NebulaChatWindow").keyValue)
            {
                chatWindow.Toggle();
            }

            ChatWindow.QueuedMessage newMessage = chatWindow.GetQueuedMessage();
            if (Multiplayer.IsActive && newMessage != null)
            {
                Multiplayer.Session.Network?.SendPacket(new NewChatMessagePacket((ChatMessageType)newMessage.ChatMessageType,
                    newMessage.MessageText, DateTime.Now, GetUserName()));
            }

            SendPostConnectionPlanetInfoMessage();
        }

        private static string GetUserName()
        {
            return Multiplayer.Session?.LocalPlayer?.Data?.Username ?? "Unknown";
        }

        private void SendPostConnectionPlanetInfoMessage()
        {
            if (sentLocation || !Multiplayer.IsActive || Multiplayer.Session.IsInLobby || Multiplayer.IsInMultiplayerMenu)
                return;
            if (GameMain.localPlanet == null && attemptsToGetLocationCountDown-- > 0)
            {
                return;
            }

            string locationStr = GameMain.localPlanet == null ? "In Space" : GameMain.localPlanet.displayName;
            Multiplayer.Session.Network.SendPacket(new NewChatMessagePacket(ChatMessageType.SystemMessage,
                $"connected ({locationStr})", DateTime.Now, GetUserName()));
            sentLocation = true;
        }

        // Queue a message to appear in chat window
        public void SendChatMessage(string text, ChatMessageType messageType)
        {
            chatWindow.SendMessageToChat(text, (int)messageType);
        }
    }
}