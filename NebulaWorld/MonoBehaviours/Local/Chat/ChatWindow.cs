#region

using System;
using System.Collections.Generic;
using System.Linq;
using NebulaModel;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Logger;
using NebulaModel.Utils;
using NebulaWorld.Chat;
using NebulaWorld.Chat.ChatLinks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

#endregion

namespace NebulaWorld.MonoBehaviours.Local.Chat;

[RequireComponent(typeof(UIWindowDrag))]
public class ChatWindow : MonoBehaviour, IChatView
{
    private const int MAX_MESSAGES = 200;

    [SerializeField] private TMP_InputField chatBox;
    [SerializeField] private RectTransform chatPanel;
    [SerializeField] private GameObject textObject;
    [SerializeField] private RectTransform notifier;
    [SerializeField] private RectTransform notifierMask;
    [SerializeField] private GameObject chatWindow;

    private readonly List<string> inputHistory = [""];
    private readonly List<TMProChatMessage> messages = [];

    internal UIWindowDrag DragTrigger;
    private int inputHistoryCursor;
    //internal string UserName;

    public bool IsActive { get; private set; }

    /// <summary>
    /// Event triggered when user submits a message
    /// </summary>
    public event Action<string> OnMessageSubmitted;

    private void Awake()
    {
        DragTrigger = GetComponent<UIWindowDrag>();

        // Subscribe to ChatService events
        ChatService.Instance.OnMessageAdded += AddMessage;
        ChatService.Instance.OnMessageRefresh += RefreshMessage;
        ChatService.Instance.OnMessageRemoved += RemoveMessage;
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
        if (!chatWindow.activeSelf)
        {
            return;
        }

        notifierMask.sizeDelta = new Vector2(chatPanel.rect.width, notifierMask.sizeDelta.y);

        if (!Input.anyKey) return;

        if (chatBox.isFocused)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                UseHistoryInput(-1);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                UseHistoryInput(+1);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                FocusInputField();
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(chatBox.text))
        {
            TrySendMessage();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
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

            Toggle(forceClosed: true);
            VFInput.UseEscape();
        }
    }

    private void UseHistoryInput(int offset)
    {
        if (chatBox.text != inputHistory[inputHistoryCursor])
        {
            // Input has changed, save to last
            inputHistoryCursor = inputHistory.Count - 1;
            inputHistory[inputHistoryCursor] = chatBox.text;
        }

        var cursor = inputHistoryCursor + offset;
        cursor = cursor < 0 ? 0 : cursor >= inputHistory.Count ? inputHistory.Count - 1 : cursor;

        if (cursor == inputHistoryCursor)
        {
            return;
        }

        chatBox.text = inputHistory[cursor];
        chatBox.MoveToEndOfLine(false, true);
        inputHistoryCursor = cursor;
    }

    private void FocusInputField()
    {
        chatBox.ActivateInputField();
        if (!chatBox.text.Equals(""))
        {
            chatBox.MoveToEndOfLine(false, true);
        }
    }

    private void TrySendMessage()
    {
        if (inputHistory.Count < 2 || chatBox.text != inputHistory[inputHistory.Count - 2])
        {
            inputHistory[inputHistory.Count - 1] = chatBox.text;
            inputHistory.Add("");
            if (inputHistory.Count > 10)
            {
                inputHistory.RemoveAt(0);
            }
            inputHistoryCursor = inputHistory.Count - 1;
        }

        // Trigger event for ChatService to process
        OnMessageSubmitted?.Invoke(chatBox.text);

        chatBox.text = "";
        // Bring cursor back to message area so they can keep typing
        FocusInputField();
    }

    #region IChatView Implementation

    public void AddMessage(RawChatMessage message)
    {
        if (messages.Count >= MAX_MESSAGES)
        {
            messages[0].DestroyMessage();
            messages.RemoveAt(0);
        }

        var chatTextObj = Instantiate(textObject, chatPanel);
        var newMsg = new TMProChatMessage(chatTextObj, message);
        messages.Add(newMsg);

        if (message.MessageType.IsPlayerMessage())
        {
            var notificationObj = Instantiate(chatTextObj, notifier);
            var notificationComponent = notificationObj.AddComponent<NotificationMessage>();
            notificationComponent.Init(newMsg.chatText.text, newMsg.chatText.color, Config.Options.NotificationDuration);
            newMsg.notificationObj = notificationObj;

            if (!chatWindow.activeSelf && Config.Options.AutoOpenChat)
            {
                // When receiving message from other players, open chat window if the option is enable
                Toggle(forceClosed: false);
            }
        }
    }

    public void RefreshMessage(RawChatMessage rawChatMessage)
    {
        foreach (var msg in messages)
        {
            if (msg.rawChatMessage == rawChatMessage)
            {
                msg.SetMessage(rawChatMessage); // Refresh the display
                return;
            }
        }
    }

    public void RemoveMessage(RawChatMessage rawChatMessage)
    {
        for (var i = messages.Count - 1; i >= 0; i--)
        {
            var msg = messages[i];
            if (msg.rawChatMessage == rawChatMessage)
            {
                msg.DestroyMessage();
                messages.RemoveAt(i);
                return;
            }
        }
    }

    public void ClearMessages(Func<RawChatMessage, bool> filter)
    {
        for (var i = messages.Count - 1; i >= 0; i--)
        {
            var msg = messages[i];
            if (filter(msg.rawChatMessage))
            {
                msg.DestroyMessage();
                messages.RemoveAt(i);
            }
        }
    }

    public void Show()
    {
        chatWindow.SetActive(true);
        notifier.gameObject.SetActive(false);
        IsActive = true;
        FocusInputField();
    }

    public void Hide()
    {
        chatWindow.SetActive(false);
        notifier.gameObject.SetActive(true);
        IsActive = false;
        ChatLinkTrigger.CloseTips();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void Toggle(bool forceClosed = false)
    {
        if (Config.Options.ChatHotkey.MainKey == KeyCode.Return)
        {
            // If player set enter as toggle hotkey, add a check for default open => close action
            // So if player is typing and hit enter, it won't close the chat window immediately
            if (!forceClosed && chatWindow.activeSelf && !string.IsNullOrEmpty(chatBox.text))
            {
                return;
            }
        }

        var desiredStatus = !forceClosed && !chatWindow.activeSelf;

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
        InsertInputField(chatBox, text);
        FocusInputField();
    }

    public bool IsPointerIn()
    {
        return DragTrigger.pointerIn;
    }

    #endregion

    #region UI Button Callbacks

    public void InsertEmoji()
    {
        EmojiPicker.Open(emoji =>
        {
            InsertInputField(chatBox, $"<sprite name=\"{emoji.UnifiedCode.ToLower()}\">");
            FocusInputField();
        });
    }

    public void InsertSprite()
    {
        var pos = new Vector2(-300, 238);
        UISignalPicker.Popup(pos, signalId =>
        {
            if (signalId <= 0)
            {
                return;
            }

            var richText = RichChatLinkRegistry.FormatShortRichText(SignalChatLinkHandler.GetLinkString(signalId));
            InsertInputField(chatBox, richText);
            FocusInputField();
        });
    }

    #endregion

    private static void InsertInputField(TMP_InputField field, string str)
    {
        if (field.m_ReadOnly)
        {
            return;
        }

        field.Delete();

        // Can't go past the character limit
        if (field.characterLimit > 0 && field.text.Length >= field.characterLimit)
        {
            return;
        }

        field.text = field.text.Insert(field.m_StringPosition, str);

        field.stringSelectPositionInternal = field.stringPositionInternal += str.Length;

        field.UpdateTouchKeyboardFromEditChanges();
        field.SendOnValueChanged();
    }
}
