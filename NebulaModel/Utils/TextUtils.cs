using System.Linq;
using System.Text.RegularExpressions;
using TMPro;

namespace NebulaModel.Utils
{
    public static class TextUtils
    {
        public static readonly string[] AllowedTags = {"b", "i", "s", "u", "indent", "link", "mark", "sprite", "sub", "sup", "color"};
        
        public static string SanitizeText(string input)
        {
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
                    string newTagString = FormatIconTags(match.Value);
                    sanitized += newTagString;
                    lastIndex = match.Index + match.Length;
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

        public static string FormatIconTags(string input)
        {
            Regex regex = new Regex(@"<sprite name=""?(\w+)""?>");
            MatchCollection matches = regex.Matches(input);
            
            if (matches.Count == 0) return input;

            string signalIdStr = matches[0].Groups[1].Value;
            int signalId = int.Parse(signalIdStr);

            return $"<link=\"signal {signalId}\">[<sprite name=\"{signalId}\"> <color=\"green\">{ProtoUtils.GetSignalDisplayName(signalId)}</color>]</link>";
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