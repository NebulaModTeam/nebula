#region

using System;
using NebulaModel.Utils;
using NebulaWorld.MonoBehaviours.Local;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace NebulaWorld.Chat;

public class SignalChatLinkHandler : IChatLinkHandler
{
    public const int Corner = 2;

    public void OnClick(string data)
    {
    }

    public void OnHover(string data, ChatLinkTrigger trigger, ref MonoBehaviour tipObject)
    {
        var signalId = GetSignalId(data);
        if (signalId <= 0)
        {
            return;
        }

        if (signalId < 1000 || signalId > 20000)
        {
            if (tipObject != null)
            {
                Object.Destroy(tipObject.gameObject);
            }
            return;
        }

        UpdateTip(trigger, ref tipObject, signalId);
    }

    public string GetIconName(string data)
    {
        var signalId = GetSignalId(data);
        return signalId <= 0 ? "Unknown" : signalId.ToString();
    }

    public string GetDisplayRichText(string data)
    {
        var signalId = GetSignalId(data);
        if (signalId <= 0)
        {
            return string.Empty;
        }

        return
            $"<link=\"signal {signalId}\">[<sprite name=\"{signalId}\"> <color=\"green\">{ProtoUtils.GetSignalDisplayName(signalId)}</color>]</link>";
    }

    public static string GetLinkString(int signalId)
    {
        return $"signal {signalId}";
    }

    private static void UpdateTip(ChatLinkTrigger trigger, ref MonoBehaviour tipObject, int signalId)
    {
        var rect = (RectTransform)trigger.transform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, trigger.camera, out var offset);
        offset -= new Vector2(rect.sizeDelta.x / 2, 3f);

        var uiitemTip = tipObject as UIItemTip;
        if (uiitemTip == null)
        {
            uiitemTip = UIItemTip.Create(signalId, Corner, offset, rect, 1, 0, UIButton.ItemTipType.Item);
            if (tipObject != null)
            {
                Object.Destroy(tipObject.gameObject);
            }

            tipObject = uiitemTip;
        }

        if (!uiitemTip.gameObject.activeSelf)
        {
            uiitemTip.gameObject.SetActive(true);
            uiitemTip.SetTip(signalId, Corner, offset, rect, 1, 0, UIButton.ItemTipType.Item);
        }

        if (uiitemTip != null && uiitemTip.isActiveAndEnabled && uiitemTip.showingItemId != signalId)
        {
            uiitemTip.SetTip(signalId, Corner, offset, rect, 1, 0, UIButton.ItemTipType.Item);
        }
    }

    private static int GetSignalId(string data)
    {
        try
        {
            return int.Parse(data);
        }
        catch (Exception)
        {
            return -1;
        }
    }
}
