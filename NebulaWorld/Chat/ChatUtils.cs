using NebulaWorld.Chat;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;

namespace NebulaModel.Utils
{
    public static class ChatUtils
    {
        public static readonly string[] AllowedTags = {"b", "i", "s", "u", "indent", "link", "mark", "sprite", "sub", "sup", "color"};
        
        public static string SanitizeText(string input)
        {
            // Matches any valid rich text tag. For example: <sprite name="hello" index=5>
            Regex regex = new Regex(@"<([/\w]+)=?[""#]?\w*""?\s?[\s\w""=]*>");
            MatchCollection matches = regex.Matches(input);

            if (matches.Count == 0) return input;

            string sanitized = "";
            int lastIndex = 0;
            
            foreach (Match match in matches)
            {
                sanitized += input.Substring(lastIndex, match.Index - lastIndex);
                lastIndex = match.Index;
                
                string tagName = match.Groups[1].Value;

                if (tagName == "sprite")
                {
                    string linkString = TryParseRichTag(match.Value);
                    if (!string.IsNullOrEmpty(linkString))
                    {
                        string newTagString = RichChatLinkRegistry.FormatFullRichText(linkString);
                        sanitized += newTagString;
                        lastIndex = match.Index + match.Length;
                    }
                }

                if (AllowedTags.Contains(tagName) || AllowedTags.Contains(tagName.Substring(1))) continue;
                
                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < input.Length)
            {
                sanitized += input.Substring(lastIndex, input.Length - lastIndex);
            }
            
            return sanitized;
        }

        private static string TryParseRichTag(string tagString)
        {
            Regex regex = new Regex(@"<sprite name=""?(\w+)""? index=""?(\w+)""?>");
            MatchCollection matches = regex.Matches(tagString);
            
            if (matches.Count == 0) return "";

            return matches[0].Groups[2].Value;
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