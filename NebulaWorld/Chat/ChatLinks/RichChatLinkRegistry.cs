using NebulaModel.Logger;
using NebulaWorld.Chat;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NebulaWorld.Chat
{
    public class RichChatLinkRegistry
    {
        private static Dictionary<string, IChatLinkHandler> handlers = new Dictionary<string, IChatLinkHandler>();

        public static void RegisterChatLinkHandler(string linkID, IChatLinkHandler handler)
        {
            if (handler == null) return;
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
            string[] splitStrings = linkString.Split(' ');
            if (splitStrings.Length != 2) return "";
            
            linkData = splitStrings[1];
            return splitStrings[0];
        }

        public static IChatLinkHandler GetChatLinkHandler(string linkID)
        {
            if (handlers.ContainsKey(linkID))
            {
                IChatLinkHandler handler = handlers[linkID];
                return handler;
            }

            return null;
        }
        
        public static string FormatFullRichText(string linkString)
        {
            string linkID = ParseRichText(linkString, out string linkData);
            IChatLinkHandler handler = GetChatLinkHandler(linkID);
            if (handler == null) return "";

            return handler.GetDisplayRichText(linkData);
        }

        public static string FormatShortRichText(string linkString)
        {
            string linkID = ParseRichText(linkString, out string linkData);
            IChatLinkHandler handler = GetChatLinkHandler(linkID);
            if (handler == null) return "";
            
            return $"<sprite name=\"{handler.GetIconName(linkData)}\" color=\"{linkString}\">";
        }
        
        static RichChatLinkRegistry()
        {
            RegisterChatLinkHandler("signal", new SignalChatLinkHandler());
        }
    }
}