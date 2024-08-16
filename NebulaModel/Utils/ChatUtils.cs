#region

using System;
using System.Linq;
using System.Text.RegularExpressions;
using NebulaModel.DataStructures.Chat;
using TMPro;
using UnityEngine;

#endregion

namespace NebulaModel.Utils;

public static class ChatUtils
{
    private const float ReferenceX = 1920;
    private const float ReferenceY = 1080;

    private static readonly string[] AllowedTags =
    {
        "b", "i", "s", "u", "indent", "link", "mark", "sprite", "sub", "sup", "color"
    };

    private static readonly Vector2[] ChatMargins = { new(10, 350), new(10, 350), new(10, 10), new(10, 100) };


    private static readonly Vector2[] ChatSizes = { new(500, 300), new(700, 420), new(800, 480) };

    public static Vector2 GetDefaultPosition(ChatPosition position, ChatSize size)
    {
        var chatSize = GetDefaultSize(size);
        var margin = ChatMargins[(int)position];
        var snapRight = ((int)position & 1) == 1;
        var snapTop = ((int)position & 2) == 2;

        float needXPos;
        float needYPos;

        if (snapRight)
        {
            needXPos = ReferenceX - margin.x - chatSize.x;
        }
        else
        {
            needXPos = margin.x;
        }

        if (snapTop)
        {
            needYPos = -margin.y;
        }
        else
        {
            needYPos = -ReferenceY + margin.y + chatSize.y;
        }

        needXPos *= Screen.width / ReferenceX;
        needYPos *= Screen.height / ReferenceY;

        return new Vector2(needXPos, needYPos);
    }

    public static Vector2 GetDefaultSize(ChatSize size)
    {
        var chatSize = ChatSizes[(int)size];
        chatSize.x *= Screen.width / ReferenceX;
        chatSize.y *= Screen.height / ReferenceY;
        return chatSize;
    }


    public static string SanitizeText(string input)
    {
        // Matches any valid rich text tag. For example: <sprite name="hello" index=5>
        var regex = new Regex("""<([/\w]+)=?["#]?\w*"?\s?[\s\w"=]*>""");

        return regex.Replace(input, match =>
        {
            var tagName = match.Groups[1].Value;
            if (AllowedTags.Contains(tagName) || AllowedTags.Contains(tagName.Substring(1)))
            {
                return match.Value;
            }
            return "";
        });
    }

    public static Color GetMessageColor(ChatMessageType messageType)
    {
        return messageType switch
        {
            ChatMessageType.PlayerMessage => Color.white,
            ChatMessageType.SystemInfoMessage => Color.cyan,
            ChatMessageType.SystemWarnMessage => new Color(1, 0.95f, 0, 1),
            ChatMessageType.BattleMessage => Color.cyan,
            ChatMessageType.CommandUsageMessage => new Color(1, 0.65f, 0, 1),
            ChatMessageType.CommandOutputMessage => new Color(0.8f, 0.8f, 0.8f, 1),
            ChatMessageType.CommandErrorMessage => Color.red,
            ChatMessageType.PlayerMessagePrivate => Color.green,
            _ => Color.white, // Default chat color is white
        };
    }

    public static bool IsPlayerMessage(this ChatMessageType type)
    {
        return type is ChatMessageType.PlayerMessage or ChatMessageType.PlayerMessagePrivate;
    }

    public static bool Contains(this string source, string toCheck, StringComparison comp)
    {
        return source?.IndexOf(toCheck, comp) >= 0;
    }

    public static void Insert(this TMP_InputField field, string str)
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
