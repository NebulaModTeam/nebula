#region

using System;
using NebulaModel;
using NebulaWorld.MonoBehaviours.Local.Chat;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace NebulaWorld.Chat.ChatLinks;

public class CopyTextChatLinkHandler : IChatLinkHandler
{
    private const int Corner = 2;

    public void OnClick(string data)
    {
        GUIUtility.systemCopyBuffer = data;
    }

    public void OnRightClick(string data)
    {
        OnClick(data);
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
        return "";
    }

    public string GetDisplayRichText(string data)
    {
        return FormatCopyString(data);
    }

    private static void UpdateTip(ChatLinkTrigger trigger, ref MonoBehaviour tipObject)
    {
        var rect = (RectTransform)trigger.transform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, trigger.camera, out var offset);
        offset -= new Vector2(rect.sizeDelta.x / 2, 3f);

        var buttonTip = tipObject as UIButtonTip;
        if (buttonTip == null)
        {
            buttonTip = UIButtonTip.Create(false, "Copy".Translate(), "Click to copy to clipboard".Translate(), Corner, offset,
                0, rect, "", "");
            if (tipObject != null)
            {
                Object.Destroy(tipObject.gameObject);
            }

            tipObject = buttonTip;
        }

        if (!buttonTip.gameObject.activeSelf)
        {
            buttonTip.gameObject.SetActive(true);
            buttonTip.SetTip(false, "Copy".Translate(), "Click to copy to clipboard".Translate(), Corner, offset, 0, rect, "",
                "");
        }

        if (buttonTip.isActiveAndEnabled && !buttonTip.titleComp.text.Equals("Copy"))
        {
            buttonTip.SetTip(false, "Copy".Translate(), "Click to copy to clipboard".Translate(), Corner, offset, 0, rect, "",
                "");
        }
    }

    public static string FormatCopyString(string data, bool isSensitive = false, Func<string, string> filter = null)
    {
        if (!isSensitive || !Config.Options.StreamerMode)
        {
            return $"<link=\"copytext {data}\"><color=\"blue\"><u>{data}</u></color></link>";
        }
        var safeText = filter != null ? filter(data) : new string('*', data.Length);
        return $"<link=\"copytext {data}\"><color=\"blue\"><u>{safeText}</u></color></link>";
    }
}
