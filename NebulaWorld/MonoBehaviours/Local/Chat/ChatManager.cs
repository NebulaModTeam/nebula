using NebulaModel;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Packets.Players;
using NebulaModel.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace NebulaWorld.MonoBehaviours.Local
{
    public class ChatManager : MonoBehaviour
    {
        private ChatWindow chatWindow;
        private Image backgroundImage;
        public static ChatManager Instance;

        private void Awake()
        {
            Instance = this;
            Transform parent = UIRoot.instance.uiGame.inventoryWindow.transform.parent;
            GameObject chatGo = parent.Find("Chat Window") ? parent.Find("Chat Window").gameObject : null;
            if (chatGo == null)
            {
                // Create chat window when there is no existing one
                GameObject prefab = AssetLoader.AssetBundle.LoadAsset<GameObject>("Assets/Prefab/ChatV2.prefab");                
                chatGo = Instantiate(prefab, parent, false);
                chatGo.name = "Chat Window";

                RectTransform trans = (RectTransform)chatGo.transform;
                MultiplayerOptions options = Config.Options;

                Vector2 defaultPos = ChatUtils.GetDefaultPosition(options.DefaultChatPosition, options.DefaultChatSize);
                Vector2 defaultSize = ChatUtils.GetDefaultSize(options.DefaultChatSize);

                trans.sizeDelta = defaultSize;
                trans.anchoredPosition = defaultPos;

                // TODO: Fix ChatV2.prefab to get rid of warnings
                GameObject backgroundGo = chatGo.transform.Find("Main/background").gameObject;
                DestroyImmediate(backgroundGo.GetComponent<TranslucentImage>());
                backgroundImage = backgroundGo.AddComponent<Image>();
                backgroundImage.color = new Color(0f, 0f, 0f, options.ChatWindowOpacity);
            }

            chatWindow = chatGo.transform.GetComponentInChildren<ChatWindow>();
            chatWindow.UserName = GetUserName();
            chatWindow.Toggle(true);
            Config.OnConfigApplied += UpdateChatPosition;
        }

        private void OnDestroy()
        {
            Config.OnConfigApplied -= UpdateChatPosition;
            Instance = null;
        }

        public static void UpdateChatPosition()
        {
            MultiplayerOptions options = Config.Options;
            Vector2 defaultPos = ChatUtils.GetDefaultPosition(options.DefaultChatPosition, options.DefaultChatSize);
            Vector2 defaultSize = ChatUtils.GetDefaultSize(options.DefaultChatSize);
            
            RectTransform trans = (RectTransform)Instance.chatWindow.transform;
            trans.anchoredPosition = defaultPos;
            trans.sizeDelta = defaultSize;
            Instance.backgroundImage.color = new Color(0f, 0f, 0f, options.ChatWindowOpacity);
        }

        void Update()
        {
            if (Config.Options.ChatHotkey.IsDown())
            {
                chatWindow.Toggle();
            }

            ChatWindow.QueuedMessage newMessage = chatWindow.GetQueuedMessage();
            if (Multiplayer.IsActive && newMessage != null)
            {
                Multiplayer.Session.Network?.SendPacket(new NewChatMessagePacket(newMessage.ChatMessageType,
                    newMessage.MessageText, DateTime.Now, GetUserName()));
            }

            if (Log.LastWarnMsg != null)
            {
                SendChatMessage(Log.LastWarnMsg, ChatMessageType.SystemWarnMessage);
                Log.LastWarnMsg = null;
            }
        }

        private static string GetUserName()
        {
            return Multiplayer.Session?.LocalPlayer?.Data?.Username ?? "Unknown";
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