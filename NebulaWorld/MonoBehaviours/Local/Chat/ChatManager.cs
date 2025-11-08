#region

using System;
using NebulaModel;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Logger;
using NebulaModel.Packets.Chat;
using NebulaModel.Utils;
using NebulaWorld.Chat;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace NebulaWorld.MonoBehaviours.Local.Chat;

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance;
    private static bool showedWelcome = false;

    private Image backgroundImage;
    private IChatView currentChatView;
    private ChatViewMode currentViewMode;

    // References to view components
    private ChatWindow tmproChatView;
    private IMGUIChatView imguiChatView;
    private GameObject chatWindowGameObject;

    private void Awake()
    {
        Instance = this;

        SwitchChatView(Config.Options.ChatViewMode, false);

        Config.OnConfigApplied += ApplyConfig;

        if (!showedWelcome)
        {
            showedWelcome = true;
            ChatService.Instance.AddMessage(
                string.Format("Welcome to Nebula multiplayer mod! Press {0} to open chat window, type /help to see all commands.".Translate(),
                    Config.Options.ChatHotkey.ToString()),
                ChatMessageType.SystemInfoMessage);
        }
    }

    private void Update()
    {
        if (Config.Options.ChatHotkey.IsDown())
        {
            currentChatView.Toggle();
        }

        // Process outgoing messages from ChatService
        if (Multiplayer.IsActive)
        {
            var newMessage = ChatService.Instance.GetQueuedMessage();
            if (newMessage != null)
            {
                Multiplayer.Session.Network?.SendPacket(new NewChatMessagePacket(
                    newMessage.MessageType,
                    newMessage.MessageText,
                    newMessage.Timestamp,
                    newMessage.UserName));
            }
        }
        else
        {
            // Discard the outgoing messages
            _ = ChatService.Instance.GetQueuedMessage();
        }

        // Handle warning messages from Log system
        if (Log.LastWarnMsg != null)
        {
            ChatService.Instance.AddMessage(Log.LastWarnMsg, ChatMessageType.SystemWarnMessage);
            Log.LastWarnMsg = null;
        }
    }

    private void OnDestroy()
    {
        Log.Debug("ChatManager destroy");
        Config.OnConfigApplied -= ApplyConfig;

        if (currentChatView != null)
        {
            currentChatView.OnMessageSubmitted -= OnUserMessageSubmitted;
        }

        Instance = null;
    }

    /// <summary>
    /// Switches between different chat view implementations
    /// </summary>
    /// <param name="viewType">The type of view to switch to</param>
    /// <param name="preserveState">Whether to preserve window state (open/closed)</param>
    public void SwitchChatView(ChatViewMode viewType, bool preserveState = true)
    {
        // Don't switch if already using this view
        if (currentViewMode == viewType && currentChatView != null)
        {
            return;
        }

        var wasActive = currentChatView?.IsActive ?? false;

        // Unsubscribe from current view
        if (currentChatView != null)
        {
            currentChatView.OnMessageSubmitted -= OnUserMessageSubmitted;
            currentChatView.Hide();

            if (tmproChatView != null)
            {
                Log.Debug("Destroy tmproChatView");
                Destroy(tmproChatView);
                Destroy(chatWindowGameObject);
                tmproChatView = null;
                chatWindowGameObject = null;
            }
            if (imguiChatView != null)
            {
                Log.Debug("Destroy imguiChatView");
                Destroy(imguiChatView);
                imguiChatView = null;
            }
        }

        // Switch view
        switch (viewType)
        {
            case ChatViewMode.TMPro:
                if (tmproChatView == null)
                {
                    InitTMProChatView();
                    if (tmproChatView == null)
                    {
                        Log.Error("TMPro ChatWindow is not available!");
                        return;
                    }
                }

                // Enable TMPro components
                tmproChatView.enabled = true;
                currentChatView = tmproChatView;
                currentViewMode = ChatViewMode.TMPro;

                Log.Info("Switched to TMPro chat view");
                break;

            case ChatViewMode.IMGUI:
                if (imguiChatView == null)
                {
                    // Add IMGUI view component to the GameObject
                    imguiChatView = gameObject.AddComponent<IMGUIChatView>();
                    if (imguiChatView == null)
                    {
                        Log.Error("IMGUI ChatView is not available!");
                        return;
                    }
                }

                // Enable IMGUI
                imguiChatView.enabled = true;
                currentChatView = imguiChatView;
                currentViewMode = ChatViewMode.IMGUI;

                Log.Info("Switched to IMGUI chat view");
                break;
        }

        // Subscribe to new view
        if (currentChatView != null)
        {
            currentChatView.OnMessageSubmitted += OnUserMessageSubmitted;
            if (preserveState && wasActive)
            {
                currentChatView.Show();
            }
            ReplayRecentMessages();
        }
    }

    private void InitTMProChatView()
    {
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
                var emojiPickerBackground = backgroundGo.AddComponent<Image>();
                emojiPickerBackground.color = new Color(0f, 0f, 0f, 1f);

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

        chatWindowGameObject = chatGo;
        chatWindowGameObject.SetActive(true);

        // Initialize both view types
        tmproChatView = chatGo.transform.GetComponentInChildren<ChatWindow>();
        if (tmproChatView == null)
        {
            Log.Error("Failed to find ChatWindow component");
        }
    }

    private void ReplayRecentMessages()
    {
        // Clear the new view first
        currentChatView.ClearMessages(_ => true);

        // Replay all messages from ChatService history
        var history = ChatService.Instance.MessageHistory;
        foreach (var message in history)
        {
            currentChatView.AddMessage(message);
        }
    }

    private void OnUserMessageSubmitted(string input)
    {
        var userName = GetUserName();
        ChatService.Instance.ProcessUserInput(input, userName);
    }

    private static void ApplyConfig()
    {
        if (Instance == null || Instance.currentChatView == null)
        {
            return;
        }

        var options = Config.Options;

        if (Instance.currentViewMode != options.ChatViewMode)
        {
            Instance.SwitchChatView(options.ChatViewMode);
        }

        // Only update position for TMPro view (IMGUI handles its own positioning)
        if (Instance.currentViewMode == ChatViewMode.TMPro && Instance.tmproChatView != null)
        {
            var defaultPos = ChatUtils.GetDefaultPosition(options.DefaultChatPosition, options.DefaultChatSize);
            var defaultSize = ChatUtils.GetDefaultSize(options.DefaultChatSize);

            var trans = (RectTransform)Instance.tmproChatView.transform;
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

    #region Public API for External Code

    /// <summary>
    /// Sends a chat message to be displayed (convenience method for external code)
    /// </summary>
    /// <param name="text">The message text</param>
    /// <param name="messageType">The type of message</param>
    public void SendChatMessage(string text, ChatMessageType messageType = ChatMessageType.SystemInfoMessage)
    {
        ChatService.Instance.AddMessage(text, messageType);
    }

    /// <summary>
    /// Inserts text into the chat input box
    /// </summary>
    /// <param name="text">The text to insert</param>
    /// <param name="forceOpenChatWindow">Whether to force open the chat window</param>
    public void InsertTextToChatbox(string text, bool forceOpenChatWindow)
    {
        if (currentChatView == null) return;

        if (!currentChatView.IsActive)
        {
            if (!forceOpenChatWindow) return;
            currentChatView.Toggle();
        }

        currentChatView.InsertText(text);
    }

    /// <summary>
    /// Checks if the pointer is currently inside the chat area
    /// </summary>
    public bool IsPointerIn()
    {
        return currentChatView?.IsPointerIn() ?? false;
    }

    /// <summary>
    /// Checks if the pointer is currently inside the chat area
    /// </summary>
    public bool IsChatViewActive()
    {
        return currentChatView?.IsActive ?? false;
    }

    #endregion
}
