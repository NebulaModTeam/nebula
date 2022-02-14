using NebulaModel.DataStructures;
using NebulaModel.Packets.Players;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace NebulaModel.Utils
{
    public static class ChatUtils
    {
        internal static readonly string[] AllowedTags = {"b", "i", "s", "u", "indent", "link", "mark", "sprite", "sub", "sup", "color"};

        internal static readonly Vector2[] ChatMargins = {
            new Vector2(10, 350),
            new Vector2(10, 350),
            new Vector2(10, 10),
            new Vector2(10, 100)
        };


        internal static readonly Vector2[] ChatSizes = {
            new Vector2(500, 300),
            new Vector2(700, 420),
            new Vector2(800, 480)
        };

        internal const float ReferenceX = 1920;
        internal const float ReferenceY = 1080;

        public static Vector2 GetDefaultPosition(ChatPosition position, ChatSize size)
        {
            Vector2 chatSize = GetDefaultSize(size);
            Vector2 margin = ChatMargins[(int)position];
            bool snapRight = ((int)position & 1) == 1;
            bool snapTop = ((int)position & 2) == 2;

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
            Vector2 chatSize = ChatSizes[(int)size];
            chatSize.x *= Screen.width / ReferenceX;
            chatSize.y *= Screen.height / ReferenceY;
            return chatSize;
        }
        
        
        public static string SanitizeText(string input)
        {
            // Matches any valid rich text tag. For example: <sprite name="hello" index=5>
            Regex regex = new Regex(@"<([/\w]+)=?[""#]?\w*""?\s?[\s\w""=]*>");

            return regex.Replace(input, match =>
            {
                string tagName = match.Groups[1].Value;
                if (AllowedTags.Contains(tagName) || AllowedTags.Contains(tagName.Substring(1)))
                {
                    return match.Value;
                }
                return "";
            });
        }

        public static Color GetMessageColor(ChatMessageType messageType)
        {
            switch (messageType)
            {
                case ChatMessageType.PlayerMessage:
                    return Color.white;
                
                case ChatMessageType.SystemMessage:
                    return new Color(1,0.95f,0,1);
                
                case ChatMessageType.CommandUsageMessage:
                    return new Color(1,0.65f,0,1);
                
                case ChatMessageType.CommandOutputMessage:
                    return new Color(0.8f,0.8f,0.8f,1);
                
                case ChatMessageType.CommandErrorMessage:
                    return Color.red;

                case ChatMessageType.PlayerMessagePrivate:
                    return Color.green;
                
                default:
                    Console.WriteLine($"Requested color for unexpected chat message type {messageType}");
                    return Color.white;
            }
        }

        public static bool IsCommandMessage(this ChatMessageType type)
        {
            return type == ChatMessageType.CommandOutputMessage ||
                   type == ChatMessageType.CommandUsageMessage ||
                   type == ChatMessageType.CommandErrorMessage ||
                   type == ChatMessageType.SystemMessage;
        }
        
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        public static void Insert(this TMP_InputField field, string str)
        {
            if (field.m_ReadOnly) return;

            field.Delete();

            // Can't go past the character limit
            if (field.characterLimit > 0 && field.text.Length >= field.characterLimit) return;

            field.text = field.text.Insert(field.m_StringPosition, str);

            field.stringSelectPositionInternal = field.stringPositionInternal += str.Length;

            field.UpdateTouchKeyboardFromEditChanges();
            field.SendOnValueChanged();
        }
    }
}