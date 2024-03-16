#region

using System.Collections.Generic;
using System.Text.RegularExpressions;
using NebulaModel.Logger;
using NebulaWorld.Chat.ChatLinks;

#endregion

namespace NebulaWorld.Chat;

public static class RichChatLinkRegistry
{
    private static readonly Dictionary<string, IChatLinkHandler> handlers = new();

    static RichChatLinkRegistry()
    {
        RegisterChatLinkHandler("signal", new SignalChatLinkHandler());
        RegisterChatLinkHandler("copytext", new CopyTextChatLinkHandler());
        RegisterChatLinkHandler("navigate", new NavigateChatLinkHandler());
    }

    private static void RegisterChatLinkHandler(string linkID, IChatLinkHandler handler)
    {
        if (handler == null)
        {
            return;
        }
        if (handlers.ContainsKey(linkID))
        {
            Log.Debug($"Can't register handler, because handler for {linkID} was already registered!");
            return;
        }

        Log.Debug($"Registering Chat Link handler for {linkID}");
        handlers.Add(linkID, handler);
    }

    public static string ParseRichText(string linkString, out string linkData)
    {
        linkData = "";
        var splitStrings = linkString.Split(' ');
        if (splitStrings.Length < 2)
        {
            return "";
        }

        linkData = linkString.Substring(splitStrings[0].Length + 1);
        return splitStrings[0];
    }

    public static IChatLinkHandler GetChatLinkHandler(string linkID)
    {
        return handlers.TryGetValue(linkID, out var handler) ? handler : null;
    }

    public static string ExpandRichTextTags(string text)
    {
        var regex = new Regex("""<sprite name="(\w+)" color="([^"]+)">""");

        return regex.Replace(text, match =>
        {
            var data = match.Groups[2].Value;
            return !string.IsNullOrEmpty(data) ? FormatFullRichText(data) : match.Value;
        });
    }

    private static string FormatFullRichText(string linkString)
    {
        var linkID = ParseRichText(linkString, out var linkData);
        var handler = GetChatLinkHandler(linkID);
        return handler == null ? "" : handler.GetDisplayRichText(linkData);
    }

    public static string FormatShortRichText(string linkString)
    {
        var linkID = ParseRichText(linkString, out var linkData);
        var handler = GetChatLinkHandler(linkID);
        return handler == null ? "" : $"<sprite name=\"{handler.GetIconName(linkData)}\" color=\"{linkString}\">";
    }
}
