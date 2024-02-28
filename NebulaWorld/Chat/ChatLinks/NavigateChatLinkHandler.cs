#region

using System.Linq;
using NebulaModel.DataStructures.Chat;
using NebulaWorld.MonoBehaviours.Local.Chat;
using UnityEngine;

#endregion

namespace NebulaWorld.Chat.ChatLinks;

public class NavigateChatLinkHandler : IChatLinkHandler
{
    const char SplitSeparator = '\t';

    public void OnClick(string data)
    {
        var substrings = data.Split(SplitSeparator);
        switch (substrings.Length)
        {
            case 2: // PlayerId
                if (!ushort.TryParse(substrings[0], out var playerId)) return;
                Multiplayer.Session.Gizmos.SetIndicatorPlayerId(playerId);
                ChatManager.Instance.SendChatMessage("Starting navigation to ".Translate() + substrings[1],
                    ChatMessageType.CommandOutputMessage);
                break;
        }
    }

    public void OnHover(string data, ChatLinkTrigger trigger, ref MonoBehaviour tipObject)
    {
        if (!string.IsNullOrEmpty(data))
        {
            UpdateTip(trigger, ref tipObject);
        }
        else if (tipObject is UIButtonTip)
        {
            Object.Destroy(tipObject.gameObject);
        }
    }


    public string GetIconName(string data)
    {
        return string.Empty;
    }

    public string GetDisplayRichText(string data)
    {
        return FormatNavigateString(data);
    }

    private static void UpdateTip(ChatLinkTrigger trigger, ref MonoBehaviour tipObject)
    {
        var rect = (RectTransform)trigger.transform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, trigger.camera, out var offset);
        offset -= new Vector2(rect.sizeDelta.x / 2, 3f);

        var buttonTip = tipObject as UIButtonTip;
        if (buttonTip == null)
        {
            buttonTip = UIButtonTip.Create(false, "Navigate".Translate(),
                "Click to create a navigate line to the target.".Translate(), 2, offset, 0, rect, "", "");
            if (tipObject != null)
            {
                Object.Destroy(tipObject.gameObject);
            }

            tipObject = buttonTip;
        }

        if (!buttonTip.gameObject.activeSelf)
        {
            buttonTip.gameObject.SetActive(true);
            buttonTip.SetTip(false, "Navigate".Translate(), "Click to create a navigate line to the target.".Translate(), 2,
                offset, 0, rect, "", "");
        }

        if (buttonTip.isActiveAndEnabled && !buttonTip.titleComp.text.Equals("Navigate"))
        {
            buttonTip.SetTip(false, "Navigate".Translate(), "Click to create a navigate line to the target.".Translate(), 2,
                offset, 0, rect, "", "");
        }
    }

    public static string FormatNavigateString(string data)
    {
        var substrings = data.Split(SplitSeparator);
        return $"<link=\"navigate {data}\"><color=\"white\"><u>{substrings[substrings.Length - 1]}</u></color></link>";
    }

    public static string FormatNavigateToPlayerString(ushort playerId, string displayString)
    {
        var data = playerId + SplitSeparator.ToString() + displayString;
        return FormatNavigateString(data);
    }
}
