#region

using System.Collections;
using System.Collections.Generic;
using NebulaModel.Utils;
using UnityEngine;

#endregion

namespace NebulaWorld.Chat;

public struct Emoji(IReadOnlyDictionary<string, object> dict)
{
    public readonly string ShortName = (string)dict["short_name"];
    public string _category = (string)dict["category"];
    public readonly string UnifiedCode = (string)dict["unified"];

    public readonly int SheetX = (int)(long)dict["sheet_x"];
    public readonly int SheetY = (int)(long)dict["sheet_y"];
    public readonly int SortOrder = (int)(long)dict["sort_order"];
}

public static class EmojiDataManager
{
    public static readonly Dictionary<string, List<Emoji>> emojies = new();
    private static bool isLoaded;

    private static void Add(Emoji emoji)
    {
        if (emojies.TryGetValue(emoji._category, out var emojy))
        {
            emojy.Add(emoji);
        }
        else
        {
            emojies[emoji._category] = [.. new[] { emoji }];
        }
    }


    public static void ParseData(TextAsset asset)
    {
        if (isLoaded)
        {
            return;
        }

        var json = "{\"frames\":" + asset.text + "}";

        if (MiniJson.Deserialize(json) is Dictionary<string, object> jObject)
        {
            var array = jObject.TryGetValue("frames", out var value) ? value as IList : null;
            if (array != null)
            {
                foreach (var rawJObject in array)
                {
                    if (rawJObject is not Dictionary<string, object> emojiData)
                    {
                        continue;
                    }

                    var emoji = new Emoji(emojiData);
                    if (emoji._category.Equals("People & Body"))
                    {
                        emoji._category = "Smileys & Emotion";
                    }
                    Add(emoji);
                }
            }
        }

        foreach (var kv in emojies)
        {
            kv.Value.Sort((emoji1, emoji2) => emoji1.SortOrder.CompareTo(emoji2.SortOrder));
        }

        isLoaded = true;
    }
}
