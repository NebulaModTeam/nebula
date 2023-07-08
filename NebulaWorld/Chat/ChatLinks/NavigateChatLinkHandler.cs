using NebulaModel.DataStructures;
using NebulaWorld.MonoBehaviours.Local;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaWorld.Chat
{
    public class NavigateChatLinkHandler : IChatLinkHandler
    {
        public void OnClick(string data)
        {
            using (Multiplayer.Session.World.GetRemotePlayersModels(out Dictionary<ushort, RemotePlayerModel> remotePlayersModels))
            {
                foreach (KeyValuePair<ushort, RemotePlayerModel> model in remotePlayersModels)
                {
                    if (model.Value.Movement.Username == data)
                    {
                        // handle indicator position update in RemotePlayerMovement.cs
                        GameMain.mainPlayer.navigation.indicatorAstroId = 100000 + model.Value.Movement.PlayerID;
                        ChatManager.Instance.SendChatMessage("Starting navigation to ".Translate() + model.Value.Movement.Username, ChatMessageType.CommandOutputMessage);
                        return;
                    }
                }
            }
        }
        public void OnHover(string data, ChatLinkTrigger trigger, ref MonoBehaviour tipObject)
        {
            if (!data.Equals(""))
            {
                UpdateTip(trigger, ref tipObject);
            }
            else if (tipObject is UIButtonTip)
            {
                Object.Destroy(tipObject.gameObject);
            }
        }

        private static void UpdateTip(ChatLinkTrigger trigger, ref MonoBehaviour tipObject)
        {
            RectTransform rect = (RectTransform)trigger.transform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, trigger.camera, out Vector2 offset);
            offset -= new Vector2(rect.sizeDelta.x / 2, 3f);

            UIButtonTip buttonTip = tipObject as UIButtonTip;
            if (buttonTip == null)
            {
                buttonTip = UIButtonTip.Create(false, "Navigate".Translate(), "Click to create a navigate line to the target.".Translate(), 2, offset, 0, rect, "", "");
                if (tipObject != null)
                {
                    Object.Destroy(tipObject.gameObject);
                }

                tipObject = buttonTip;
            }

            if (!buttonTip.gameObject.activeSelf)
            {
                buttonTip.gameObject.SetActive(true);
                buttonTip.SetTip(false, "Navigate".Translate(), "Click to create a navigate line to the target.".Translate(), 2, offset, 0, rect, "", "");
            }

            if (buttonTip != null && buttonTip.isActiveAndEnabled && !buttonTip.titleComp.Equals("Navigate"))
            {
                buttonTip.SetTip(false, "Navigate".Translate(), "Click to create a navigate line to the target.".Translate(), 2, offset, 0, rect, "", "");
            }
        }


        public string GetIconName(string data)
        {
            return "";
        }
        public string GetDisplayRichText(string data)
        {
            return FormatNavigateString(data);
        }
        public static string FormatNavigateString(string data)
        {
            return $"<link=\"navigate {data}\"><color=\"white\"><u>{data}</u></color></link>";
        }
    }
}
