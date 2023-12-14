#region

using NebulaModel.DataStructures;
using NebulaWorld.MonoBehaviours.Local;
using NebulaWorld.MonoBehaviours.Local.Chat;
using UnityEngine;

#endregion

namespace NebulaWorld.Chat;

public class NavigateChatLinkHandler : IChatLinkHandler
{
    public void OnClick(string data)
    {
        using (Multiplayer.Session.World.GetRemotePlayersModels(out var remotePlayersModels))
        {
            foreach (var model in remotePlayersModels)
            {
                if (model.Value.Movement.Username == data)
                {
                    // handle indicator position update in RemotePlayerMovement.cs
                    GameMain.mainPlayer.navigation.indicatorAstroId = 100000 + model.Value.Movement.PlayerID;
                    ChatManager.Instance.SendChatMessage("Starting navigation to ".Translate() + model.Value.Movement.Username,
                        ChatMessageType.CommandOutputMessage);
                    return;
                }
            }
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
        return $"<link=\"navigate {data}\"><color=\"white\"><u>{data}</u></color></link>";
    }
}
