#region

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NebulaModel;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Utils;
using NebulaWorld.Chat;
using UnityEngine;

#endregion

namespace NebulaWorld.MonoBehaviours.Local.Chat;

/// <summary>
/// IMGUI-based chat view implementation
/// </summary>
public class IMGUIChatView : MonoBehaviour, IChatView
{
    private const int MAX_MESSAGES = 200;
    private const float FADE_DURATION = 2f;
    private const int FONT_SIZE = 16;

    // Window state
    private Rect windowRect = new(20, 350, 500, 300);
    private Rect resizeRect = new();
    private bool isResizing = false;
    private Vector2 resizeStart;

    // Message display
    private readonly List<DisplayMessage> messages = new();
    private readonly List<NotificationMessage> notifications = new();
    private Vector2 scrollPosition = Vector2.zero;

    // Input state
    private string inputText = "";
    private readonly List<string> inputHistory = new() { "" };
    private int inputHistoryCursor = 0;
    private bool toFocusInputField = false;
    private bool hasInputFocus = false;

    // UI state
    private bool isPointerInside = false;
    private GUIStyle messageStyle;
    private GUIStyle windowStyle;
    private GUIStyle notificationStyle;
    private GUIStyle inputFieldStyle;

    public bool IsActive { get; private set; }

    /// <summary>
    /// Event triggered when user submits a message
    /// </summary>
    public event Action<string> OnMessageSubmitted;

    private void Awake()
    {
        // Subscribe to ChatService events
        ChatService.Instance.OnMessageAdded += AddMessage;
        ChatService.Instance.OnMessageRefresh += RefreshMessage;
        ChatService.Instance.OnMessageRemoved += RemoveMessage;

        windowRect.position = new Vector2(20, Screen.height - 550);
    }

    private void OnDestroy()
    {
        // Unsubscribe from ChatService events
        ChatService.Instance.OnMessageAdded -= AddMessage;
        ChatService.Instance.OnMessageRefresh -= RefreshMessage;
        ChatService.Instance.OnMessageRemoved -= RemoveMessage;
    }

    private void Update()
    {
        if (!IsActive)
        {
            UpdateNotifications();
            return;
        }
    }

    private void OnGUI()
    {
        var originSkin = GUI.skin;
        GUI.skin = null;

        // Draw notifications when window is closed
        if (!IsActive)
        {
            DrawNotifications();
            GUI.skin = originSkin;
            return;
        }

        // Draw main chat window
        windowRect = GUILayout.Window(
            GetInstanceID(),
            windowRect,
            DrawChatWindow,
            "Chat Window",
            windowStyle ?? GUI.skin.window);

        // Draw resize handle
        DrawResizeHandle();

        isPointerInside = windowRect.Contains(Event.current.mousePosition);

        GUI.skin = originSkin;
    }

    #region UI Rendering

    private void EnsureStylesInitialized()
    {
        if (messageStyle == null)
        {
            // Message style
            messageStyle = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true,
                richText = true,
                alignment = TextAnchor.UpperLeft,
                fontSize = FONT_SIZE
            };

            // Window style with semi-transparent background
            windowStyle = new GUIStyle(GUI.skin.window);
            var bgTexture = CreateColorTexture(new Color(0f, 0f, 0f, Config.Options.ChatWindowOpacity));

            windowStyle.normal.background = bgTexture;
            windowStyle.onNormal.background = bgTexture;
            windowStyle.focused.background = bgTexture;
            windowStyle.onFocused.background = bgTexture;
            windowStyle.active.background = bgTexture;
            windowStyle.onActive.background = bgTexture;

            // Notification style
            notificationStyle = new GUIStyle(GUI.skin.box)
            {
                wordWrap = true,
                richText = true,
                alignment = TextAnchor.UpperLeft,
                fontSize = FONT_SIZE,
                normal = { background = CreateColorTexture(new Color(0f, 0f, 0f, 0.3f)) }
            };

            // InputField style
            inputFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                wordWrap = true,
                richText = true,
                alignment = TextAnchor.UpperLeft,
                fontSize = FONT_SIZE
            };
        }
    }

    private static Texture2D CreateColorTexture(Color color)
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private void DrawChatWindow(int windowID)
    {
        EnsureStylesInitialized();

        GUILayout.BeginVertical();

        // Messages scroll view
        scrollPosition = GUILayout.BeginScrollView(
            scrollPosition,
            GUILayout.ExpandWidth(true),
            GUILayout.ExpandHeight(true));

        foreach (var msg in messages)
        {
            DrawMessage(msg);
        }

        GUILayout.EndScrollView();

        // Input area
        GUILayout.BeginHorizontal();

        HandleKeyEvent();
        GUI.SetNextControlName("ChatInput");
        inputText = GUILayout.TextField(inputText, inputFieldStyle);
        // Handle input focus
        if (toFocusInputField)
        {
            GUI.FocusControl("ChatInput");
            toFocusInputField = false;
        }
        hasInputFocus = GUI.GetNameOfFocusedControl() == "ChatInput";

        if (GUILayout.Button("Send", GUILayout.ExpandWidth(false)))
        {
            SubmitMessage();
        }

        if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
        {
            Hide();
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        // Make window draggable
        GUI.DragWindow(new Rect(0, 0, windowRect.width, 20));
    }


    private void HandleKeyEvent()
    {
        var e = Event.current;
        if (e.type != EventType.KeyDown) return;

        if (e.keyCode == Config.Options.ChatHotkey.MainKey && Config.Options.ChatHotkey.MainKey != KeyCode.Return && Config.Options.ChatHotkey.MainKey != KeyCode.KeypadEnter)
        {
            Toggle(forceClosed: true);
            e.Use();
            return;
        }

        switch (e.keyCode)
        {
            case KeyCode.UpArrow:
                if (hasInputFocus)
                {
                    UseHistoryInput(-1);
                    e.Use();
                }
                return;

            case KeyCode.DownArrow:
                if (hasInputFocus)
                {
                    UseHistoryInput(1);
                    e.Use();
                }
                return;

            case KeyCode.Return:
            case KeyCode.KeypadEnter:
                if (!string.IsNullOrEmpty(inputText))
                {
                    SubmitMessage();
                }
                else
                {
                    Toggle(forceClosed: true);
                }
                e.Use();
                return;

            case KeyCode.Escape:
                if (UISignalPicker.isOpened)
                {
                    UISignalPicker.Close();
                    VFInput.UseEscape();
                    return;
                }
                Toggle(forceClosed: true);
                VFInput.UseEscape();
                return;
        }
    }

    private void DrawMessage(DisplayMessage msg)
    {
        var oldColor = GUI.contentColor;
        GUI.contentColor = msg.Color;
        GUILayout.Label(msg.Text, messageStyle);
        GUI.contentColor = oldColor;
    }

    private void DrawNotifications()
    {
        EnsureStylesInitialized();
        var windowWidth = windowRect.width;
        var xOffset = windowRect.xMin + 12;
        var yOffset = windowRect.yMax - 63;

        var oldColor = GUI.color;
        for (var i = notifications.Count - 1; i >= 0; i--)
        {
            var notification = notifications[i];
            var color = notification.Color;
            color.a = notification.GetAlpha();
            GUI.color = color;
            var content = new GUIContent(notification.Text);
            var height = notificationStyle.CalcHeight(content, windowWidth);
            notificationStyle.CalcMinMaxWidth(content, out _, out var width);
            width = Math.Min(width, windowWidth);
            var rect = new Rect(xOffset, yOffset, width, height);
            GUI.Box(rect, notification.Text, notificationStyle);
            yOffset -= height;
            if (yOffset < windowRect.yMin) break;
        }
        GUI.color = oldColor;
    }

    private void DrawResizeHandle()
    {
        resizeRect = new Rect(
            windowRect.x + windowRect.width - 10,
            windowRect.y + windowRect.height - 10,
            20,
            20);

        var e = Event.current;
        var isHoveringResizeRect = resizeRect.Contains(e.mousePosition);

        if (isHoveringResizeRect)
        {
            GUI.Box(resizeRect, "↘");
        }

        if (e.type == EventType.MouseDown && resizeRect.Contains(e.mousePosition))
        {
            isResizing = true;
            resizeStart = Event.current.mousePosition;
        }

        if (isResizing)
        {
            if (e.type == EventType.MouseDrag)
            {
                var delta = e.mousePosition - resizeStart;
                windowRect.width = Mathf.Max(150, windowRect.width + delta.x);
                windowRect.height = Mathf.Max(50, windowRect.height + delta.y);
                resizeStart = e.mousePosition;
            }
            else if (e.type == EventType.MouseUp)
            {
                isResizing = false;
            }
        }
    }

    #endregion

    private void SubmitMessage()
    {
        if (string.IsNullOrEmpty(inputText)) return;

        // Add to history
        if (inputHistory.Count < 2 || inputText != inputHistory[inputHistory.Count - 2])
        {
            inputHistory[inputHistory.Count - 1] = inputText;
            inputHistory.Add("");
            if (inputHistory.Count > 10)
            {
                inputHistory.RemoveAt(0);
            }
            inputHistoryCursor = inputHistory.Count - 1;
        }

        // Trigger submission event
        OnMessageSubmitted?.Invoke(inputText);

        inputText = "";
        toFocusInputField = true;
    }

    private void UseHistoryInput(int offset)
    {
        if (inputText != inputHistory[inputHistoryCursor])
        {
            inputHistoryCursor = inputHistory.Count - 1;
            inputHistory[inputHistoryCursor] = inputText;
        }

        var cursor = inputHistoryCursor + offset;
        cursor = Mathf.Clamp(cursor, 0, inputHistory.Count - 1);

        if (cursor != inputHistoryCursor)
        {
            inputText = inputHistory[cursor];
            inputHistoryCursor = cursor;
        }
    }

    private void UpdateNotifications()
    {
        for (var i = notifications.Count - 1; i >= 0; i--)
        {
            if (notifications[i].IsExpired())
            {
                notifications.RemoveAt(i);
            }
        }
    }


    #region IChatView Implementation

    public void AddMessage(RawChatMessage rawChatMessage)
    {
        if (messages.Count > MAX_MESSAGES)
        {
            messages.RemoveAt(0);
        }

        var displayMsg = new DisplayMessage(rawChatMessage);
        messages.Add(displayMsg);

        if (rawChatMessage.MessageType.IsPlayerMessage())
        {
            var notificationMsg = new NotificationMessage(
                displayMsg.Text,
                displayMsg.Color,
                Config.Options.NotificationDuration);
            notifications.Add(notificationMsg);
        }

        // Auto scroll to bottom
        scrollPosition.y = float.MaxValue;
    }

    public void RefreshMessage(RawChatMessage rawChatMessage)
    {
        for (var i = messages.Count - 1; i >= 0; i--)
        {
            var msg = messages[i];
            if (msg.RawChatMessage == rawChatMessage)
            {
                msg.SetMessage(rawChatMessage);
                return;
            }
        }
    }

    public void RemoveMessage(RawChatMessage rawChatMessage)
    {
        for (var i = messages.Count - 1; i >= 0; i--)
        {
            var msg = messages[i];
            if (msg.RawChatMessage == rawChatMessage)
            {
                messages.RemoveAt(i);
                return;
            }
        }
        notifications.Clear();
    }

    public void ClearMessages(Func<RawChatMessage, bool> predicate)
    {
        messages.RemoveAll(msg => predicate(msg.RawChatMessage));
        notifications.Clear();
    }

    public void Show()
    {
        IsActive = true;
        toFocusInputField = true;
    }

    public void Hide()
    {
        IsActive = false;
        GUI.FocusControl(null);
    }

    public void Toggle(bool forceClosed = false)
    {
        if (Config.Options.ChatHotkey.MainKey == KeyCode.Return)
        {
            // If player set enter as toggle hotkey, prevent closing while typing
            if (forceClosed == false && IsActive && !string.IsNullOrEmpty(inputText))
            {
                return;
            }
        }

        var desiredStatus = !forceClosed && !IsActive;

        if (desiredStatus)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    public void InsertText(string text)
    {
        text = Regex.Replace(text, @"<[^>]+>", "");
        inputText += text;
        toFocusInputField = true;
    }

    public bool IsPointerIn()
    {
        return isPointerInside;
    }

    #endregion


    #region Helper Classes

    private class DisplayMessage
    {
        public RawChatMessage RawChatMessage { get; private set; }
        public string Text { get; private set; }
        public Color Color { get; private set; }
        public ChatMessageType MessageType { get; private set; }

        public DisplayMessage(RawChatMessage rawChatMessage)
        {
            SetMessage(rawChatMessage);
        }

        public void SetMessage(RawChatMessage rawChatMessage)
        {
            RawChatMessage = rawChatMessage;
            var formattedText = ChatUtils.FormatMessage(rawChatMessage);
            if (rawChatMessage.MessageType.IsPlayerMessage())
            {
                formattedText = ChatUtils.SanitizeText(formattedText);
            }
            // TODO: Handle the chat links
            formattedText = StripAllTags(formattedText);
            Text = formattedText;
            MessageType = rawChatMessage.MessageType;
            Color = ChatUtils.GetMessageColor(rawChatMessage.MessageType);
        }

        private static string StripAllTags(string text)
        {
            return Regex.Replace(text, @"<[^>]+>", "");
        }
    }

    private class NotificationMessage
    {
        public string Text { get; }
        public Color Color { get; }
        public float Height { get; }
        public float Width { get; }
        private readonly float startTime = Time.time;
        private readonly float duration;

        public NotificationMessage(string text, Color color, float duration)
        {
            Text = text;
            Color = color;
            this.duration = duration;
        }

        public float GetAlpha()
        {
            var elapsed = Time.time - startTime;
            if (elapsed < duration)
            {
                return 1f;
            }
            var fadeElapsed = elapsed - duration;
            if (fadeElapsed < FADE_DURATION)
            {
                return 1f - (fadeElapsed / FADE_DURATION);
            }
            return 0f;
        }

        public bool IsExpired()
        {
            return Time.time - startTime > duration + FADE_DURATION;
        }
    }

    #endregion
}
