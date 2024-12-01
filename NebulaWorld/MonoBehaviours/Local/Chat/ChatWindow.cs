#region

using System;
using System.Collections.Generic;
using System.Linq;
using NebulaModel;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Utils;
using NebulaWorld.Chat;
using NebulaWorld.Chat.ChatLinks;
using NebulaWorld.Chat.Commands;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

#endregion

namespace NebulaWorld.MonoBehaviours.Local.Chat;

public class ChatWindow : MonoBehaviour
{
    private const int MAX_MESSAGES = 200;

    [SerializeField] private TMP_InputField chatBox;
    [SerializeField] private RectTransform chatPanel;
    [SerializeField] private GameObject textObject;
    [SerializeField] private RectTransform notifier;
    [SerializeField] private RectTransform notifierMask;

    [SerializeField] private GameObject chatWindow;
    private readonly List<string> inputHistory = [""];
    private readonly List<ChatMessage> messages = [];
    private readonly Queue<QueuedMessage> outgoingMessages = new(5);
    internal UIWindowDrag DragTrigger;
    private int inputHistoryCursor;


    internal string UserName;
    public bool IsActive { get; private set; }


    private void Awake()
    {
        DragTrigger = GetComponent<UIWindowDrag>();
    }

    private void Update()
    {
        if (!chatWindow.activeSelf)
        {
            return;
        }

        notifierMask.sizeDelta = new Vector2(chatPanel.rect.width, notifierMask.sizeDelta.y);

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

        if (chatBox.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                TrySendMessage();
            }
            else
            {
                if (!chatBox.isFocused && Input.GetKeyDown(KeyCode.Return))
                {
                    FocusInputField();
                }
            }
        }

        if (!Input.GetKeyDown(KeyCode.Escape))
        {
            return;
        }
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

        Toggle(true);
        VFInput.UseEscape();
    }

    private void UseHistoryInput(int offset)
    {
        if (chatBox.text != inputHistory[inputHistoryCursor])
        {
            // input has changed, save to last
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

        if (chatBox.text.StartsWith(ChatCommandRegistry.CommandPrefix))
        {
            var arguments = chatBox.text.Substring(1).Split(' ');
            if (arguments.Length > 0)
            {
                var commandName = arguments[0];
                var handler = ChatCommandRegistry.GetCommandHandler(commandName);
                if (handler != null)
                {
                    try
                    {
                        handler.Execute(this, arguments.Skip(1).ToArray());
                    }
                    catch (ChatCommandUsageException e)
                    {
                        SendLocalChatMessage(
                            $"Invalid usage: {e.Message}! Usage: {ChatCommandRegistry.CommandPrefix}{commandName} {handler.GetUsage()}",
                            ChatMessageType.CommandUsageMessage);
                    }
                }
                else
                {
                    SendLocalChatMessage($"Unknown command {commandName}. Use /help to get list of commands",
                        ChatMessageType.CommandUsageMessage);
                }
            }
        }
        else
        {
            BroadcastChatMessage(chatBox.text);
        }
        chatBox.text = "";
        // bring cursor back to message area so they can keep typing
        FocusInputField();
    }

    private void BroadcastChatMessage(string message, ChatMessageType chatMessageType = ChatMessageType.PlayerMessage)
    {
        QueueOutgoingChatMessage(message, chatMessageType);
        var formattedMessage = ChatManager.FormatChatMessage(DateTime.Now, UserName, message);
        SendLocalChatMessage(formattedMessage, chatMessageType);
    }

    private void QueueOutgoingChatMessage(string message, ChatMessageType chatMesageType)
    {
        outgoingMessages.Enqueue(new QueuedMessage { MessageText = message, ChatMessageType = chatMesageType });
    }

    public ChatMessage SendLocalChatMessage(string text, ChatMessageType messageType)
    {
        if (messageType.IsPlayerMessage())
        {
            text = ChatUtils.SanitizeText(text);
        }
        else
        {
            switch (messageType)
            {
                case ChatMessageType.SystemInfoMessage when !Config.Options.EnableInfoMessage:
                case ChatMessageType.SystemWarnMessage when !Config.Options.EnableWarnMessage:
                case ChatMessageType.BattleMessage when !Config.Options.EnableBattleMessage:
                    return null;
            }
        }

        text = RichChatLinkRegistry.ExpandRichTextTags(text);

        if (messages.Count > MAX_MESSAGES)
        {
            messages[0].DestroyMessage();
            messages.Remove(messages[0]);
        }

        var textObj = Instantiate(textObject, chatPanel);
        var newMsg = new ChatMessage(textObj, text, messageType);

        var notificationMsg = Instantiate(textObj, notifier);
        newMsg.notificationText = notificationMsg.GetComponent<TMP_Text>();
        var message = notificationMsg.AddComponent<NotificationMessage>();
        message.Init(Config.Options.NotificationDuration);

        messages.Add(newMsg);

        if (chatWindow.activeSelf)
        {
            return newMsg;
        }
        if (Config.Options.AutoOpenChat && messageType.IsPlayerMessage())
        {
            Toggle(false, false);
        }

        return newMsg;
    }

    public void ClearChat()
    {
        foreach (var message in messages)
        {
            message.DestroyMessage();
        }
        messages.Clear();
    }

    public void ClearChat(Func<ChatMessage, bool> filter)
    {
        for (var i = 0; i < messages.Count; i++)
        {
            var message = messages[i];
            if (!filter(message))
            {
                continue;
            }

            message.DestroyMessage();
            messages.RemoveAt(i);
            i--;
        }
    }


    public void Toggle(bool forceClosed = false, bool focusField = true)
    {
        if (Config.Options.ChatHotkey.MainKey == KeyCode.Return)
        {
            // If player set enter as toggle hotkey, add a check for default open => close action
            // So if player is typing and hit enter, it won't close the chat window immediately
            if (forceClosed == false && chatWindow.activeSelf && !string.IsNullOrEmpty(chatBox.text)) return;
        }

        var desiredStatus = !forceClosed && !chatWindow.activeSelf;
        chatWindow.SetActive(desiredStatus);
        notifier.gameObject.SetActive(!desiredStatus);
        IsActive = desiredStatus;
        if (chatWindow.activeSelf)
        {
            // when the window is activated we assume user wants to type right away
            if (focusField)
            {
                FocusInputField();
            }
        }
        else
        {
            ChatLinkTrigger.CloseTips();
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void InsertEmoji()
    {
        EmojiPicker.Open(emoji =>
        {
            chatBox.Insert($"<sprite name=\"{emoji.UnifiedCode.ToLower()}\">");
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
            chatBox.Insert(richText);
            FocusInputField();
        });
    }

    public void InsertText(string richText)
    {
        chatBox.Insert(richText);
        FocusInputField();
    }

    public QueuedMessage GetQueuedMessage()
    {
        return outgoingMessages.Count > 0 ? outgoingMessages.Dequeue() : null;
    }

    public class QueuedMessage
    {
        public ChatMessageType ChatMessageType;
        public string MessageText;
    }
}
