using System.Linq;
using System.Text.RegularExpressions;

namespace NebulaModel.Utils
{
    public class TextUtils
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
                if (AllowedTags.Contains(tagName) || AllowedTags.Contains(tagName.Substring(1))) continue;
                
                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < input.Length)
            {
                sanitized += input.Substring(lastIndex, input.Length - lastIndex);
            }
            
            return sanitized;
        }
    }
}