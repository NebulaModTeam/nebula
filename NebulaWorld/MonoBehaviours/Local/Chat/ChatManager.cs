#region

using System;
using System.Linq;
using NebulaModel;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Logger;
using NebulaModel.Packets.Chat;
using NebulaModel.Utils;
using NebulaWorld.Chat.ChatLinks;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace NebulaWorld.MonoBehaviours.Local.Chat;

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance;
    private static bool showedWelcome = false;
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

        if (!showedWelcome)
        {
            showedWelcome = true;
            SendChatMessage(string.Format("Welcome to Nebula multiplayer mod! Press {0} to open chat window, type /help to see all commands.".Translate()
                , Config.Options.ChatHotkey.ToString()), ChatMessageType.SystemInfoMessage);
        }
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

    public static string FormatChatMessage(in DateTime sentTime, string userName, string messageBody)
    {
        // format: $"[{sentTime:HH:mm}] {userName} : {messageBody}

        var formattedString = "";
        if (!string.IsNullOrEmpty(userName))
        {
            ushort playerId = 0;
            if (Multiplayer.IsActive)
            {
                using (Multiplayer.Session.World.GetRemotePlayersModels(out var remotePlayersModels))
                {
                    playerId = remotePlayersModels.FirstOrDefault(x => x.Value.Username == userName).Key;
                }
            }
            if (playerId > 0)
            {
                formattedString = NavigateChatLinkHandler.FormatNavigateToPlayerString(playerId, userName);
            }
            else
            {
                formattedString = userName;
            }
        }
        if (Config.Options.EnableTimestamp)
        {
            formattedString = $"[{sentTime:HH:mm}] {formattedString} : ";
        }
        else if (!string.IsNullOrEmpty(formattedString))
        {
            formattedString += " : ";
        }
        return formattedString + messageBody;
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

    public void InsetTextToChatbox(string text, bool forceOpenChatWindow)
    {
        if (chatWindow == null) return;
        if (!chatWindow.IsActive)
        {
            if (!forceOpenChatWindow) return;
            chatWindow.Toggle();
        }
        chatWindow.InsertText(text);
    }

    public bool IsPointerIn()
    {
        return chatWindow.DragTrigger.pointerIn;
    }
}
