using CommonAPI;
using CommonAPI.Systems;
using NebulaModel;
using NebulaModel.DataStructures;
using NebulaModel.Packets.Players;
using NebulaModel.Utils;
using System;
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
            
            RectTransform trans = (RectTransform)chatGo.transform;
            MultiplayerOptions options = Config.Options;

            Vector2 defaultPos = ChatUtils.GetDefaultPosition(options.DefaultChatPosition, options.DefaultChatSize);
            Vector2 defaultSize = ChatUtils.GetDefaultSize(options.DefaultChatSize);
            
            trans.sizeDelta = defaultSize;
            trans.anchoredPosition = defaultPos;

            chatGo.GetComponent<UIWindowResize>().minSize = ChatUtils.GetDefaultSize(ChatSize.Small);

            chatWindow = chatGo.transform.GetComponentInChildren<ChatWindow>();
            chatWindow.userName = GetUserName();
            chatWindow.Toggle(true);
            Config.OnConfigApplied += UpdateChatPosition;
        }

        private void OnDestroy()
        {
            Config.OnConfigApplied -= UpdateChatPosition;
        }

        public static void UpdateChatPosition()
        {
            MultiplayerOptions options = Config.Options;
            Vector2 defaultPos = ChatUtils.GetDefaultPosition(options.DefaultChatPosition, options.DefaultChatSize);
            Vector2 defaultSize = ChatUtils.GetDefaultSize(options.DefaultChatSize);
            
            RectTransform trans = (RectTransform)Instance.chatWindow.transform;
            trans.anchoredPosition = defaultPos;
            trans.sizeDelta = defaultSize;
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
                Multiplayer.Session.Network?.SendPacket(new NewChatMessagePacket(newMessage.ChatMessageType,
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
            chatWindow.SendLocalChatMessage(text, messageType);
        }

        public bool IsPointerIn()
        {
            return chatWindow.DragTrigger.pointerIn;
        }
    }
}