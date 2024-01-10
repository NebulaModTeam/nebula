#region

using System;
using NebulaModel;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Logger;
using NebulaModel.Packets.Chat;
using NebulaModel.Utils;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace NebulaWorld.MonoBehaviours.Local.Chat;

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance;
    private Image backgroundImage;
    private ChatWindow chatWindow;

    private void Awake()
    {
        Instance = this;
        var parent = UIRoot.instance.uiGame.inventoryWindow.transform.parent;
        var chatGo = parent.Find("Chat Window") ? parent.Find("Chat Window").gameObject : null;
        if (chatGo == null)
        {
            // Create chat window when there is no existing one
            var prefab = AssetLoader.AssetBundle.LoadAsset<GameObject>("Assets/Prefab/ChatV2.prefab");
            chatGo = Instantiate(prefab, parent, false);
            chatGo.name = "Chat Window";

            var trans = (RectTransform)chatGo.transform;
            var options = Config.Options;

            var defaultPos = ChatUtils.GetDefaultPosition(options.DefaultChatPosition, options.DefaultChatSize);
            var defaultSize = ChatUtils.GetDefaultSize(options.DefaultChatSize);

            trans.sizeDelta = defaultSize;
            trans.anchoredPosition = defaultPos;

            try
            {
                // TODO: Fix ChatV2.prefab to get rid of warnings
                var removeComponent = chatGo.GetComponent("CommonAPI.MaterialFixer");
                if (removeComponent != null)
                {
                    Destroy(removeComponent);
                }

                var backgroundGo = chatGo.transform.Find("Main/background").gameObject;
                DestroyImmediate(backgroundGo.GetComponent<TranslucentImage>());
                backgroundImage = backgroundGo.AddComponent<Image>();
                backgroundImage.color = new Color(0f, 0f, 0f, options.ChatWindowOpacity);

                backgroundGo = chatGo.transform.Find("Main/EmojiPicker/background").gameObject;
                DestroyImmediate(backgroundGo.GetComponent<TranslucentImage>());
                var EmojiPickerbackground = backgroundGo.AddComponent<Image>();
                EmojiPickerbackground.color = new Color(0f, 0f, 0f, 1f);

                var notifications = chatGo.transform.Find("NotificationsMask/Notifications");
                notifications.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

                var uiWindowDrag = chatGo.GetComponent<UIWindowDrag>();
                uiWindowDrag.screenRect = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows").GetComponent<RectTransform>();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        chatWindow = chatGo.transform.GetComponentInChildren<ChatWindow>();
        if (chatWindow == null)
        {
            Log.Error("Failed to find ChatWindow component");
            this.enabled = false;
            return;
        }
        chatWindow.UserName = GetUserName();
        chatWindow.Toggle(true);
        Config.OnConfigApplied += UpdateChatPosition;
    }

    private void Update()
    {
        if (Config.Options.ChatHotkey.IsDown())
        {
            chatWindow.Toggle();
        }

        var newMessage = chatWindow.GetQueuedMessage();
        if (Multiplayer.IsActive && newMessage != null)
        {
            Multiplayer.Session.Network?.SendPacket(new NewChatMessagePacket(newMessage.ChatMessageType,
                newMessage.MessageText, DateTime.Now, GetUserName()));
        }

        if (Log.LastWarnMsg == null)
        {
            return;
        }
        SendChatMessage(Log.LastWarnMsg, ChatMessageType.SystemWarnMessage);
        Log.LastWarnMsg = null;
    }

    private void OnDestroy()
    {
        Config.OnConfigApplied -= UpdateChatPosition;
        Instance = null;
    }

    private static void UpdateChatPosition()
    {
        var options = Config.Options;
        var defaultPos = ChatUtils.GetDefaultPosition(options.DefaultChatPosition, options.DefaultChatSize);
        var defaultSize = ChatUtils.GetDefaultSize(options.DefaultChatSize);

        if (Instance.chatWindow != null)
        {
            var trans = (RectTransform)Instance.chatWindow.transform;
            trans.anchoredPosition = defaultPos;
            trans.sizeDelta = defaultSize;
        }

        if (Instance.backgroundImage != null)
        {
            Instance.backgroundImage.color = new Color(0f, 0f, 0f, options.ChatWindowOpacity);
        }
    }

    private static string GetUserName()
    {
        return Multiplayer.Session?.LocalPlayer?.Data?.Username ?? "Unknown";
    }


    // Queue a message to appear in chat window
    public void SendChatMessage(string text, ChatMessageType messageType)
    {
        if (chatWindow == null)
        {
            Log.Error("Failed to find ChatWindow component");
            return;
        }
        chatWindow.SendLocalChatMessage(text, messageType);
    }

    public bool IsPointerIn()
    {
        return chatWindow.DragTrigger.pointerIn;
    }
}
