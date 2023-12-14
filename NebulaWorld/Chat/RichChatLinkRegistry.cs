#region

using System.Collections.Generic;
using System.Text.RegularExpressions;
using NebulaModel.Logger;

#endregion

namespace NebulaWorld.Chat;

public class RichChatLinkRegistry
{
    private static readonly Dictionary<string, IChatLinkHandler> handlers = new();

    static RichChatLinkRegistry()
    {
        RegisterChatLinkHandler("signal", new SignalChatLinkHandler());
        RegisterChatLinkHandler("copytext", new CopyTextChatLinkHandler());
        RegisterChatLinkHandler("navigate", new NavigateChatLinkHandler());
    }

    public static void RegisterChatLinkHandler(string linkID, IChatLinkHandler handler)
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
        if (splitStrings.Length != 2)
        {
            return "";
        }

        linkData = splitStrings[1];
        return splitStrings[0];
    }

    public static IChatLinkHandler GetChatLinkHandler(string linkID)
    {
        if (handlers.ContainsKey(linkID))
        {
            var handler = handlers[linkID];
            return handler;
        }

        return null;
    }

    public static string ExpandRichTextTags(string text)
    {
        var regex = new Regex(@"<sprite name=""(\w+)"" color=""([^""]+)"">");

        return regex.Replace(text, match =>
        {
            var data = match.Groups[2].Value;
            if (!string.IsNullOrEmpty(data))
            {
                return FormatFullRichText(data);
            }

            return match.Value;
        });
    }

    public static string FormatFullRichText(string linkString)
    {
        var linkID = ParseRichText(linkString, out var linkData);
        var handler = GetChatLinkHandler(linkID);
        if (handler == null)
        {
            return "";
        }

        return handler.GetDisplayRichText(linkData);
    }

    public static string FormatShortRichText(string linkString)
    {
        var linkID = ParseRichText(linkString, out var linkData);
        var handler = GetChatLinkHandler(linkID);
        if (handler == null)
        {
            return "";
        }

        return $"<sprite name=\"{handler.GetIconName(linkData)}\" color=\"{linkString}\">";
    }
}
