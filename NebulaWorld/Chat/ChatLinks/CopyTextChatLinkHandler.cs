using NebulaWorld.MonoBehaviours.Local;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NebulaWorld.Chat
{
    public class CopyTextChatLinkHandler : IChatLinkHandler
    {
        public const int Corner = 2;
        
        public void OnClick(string data)
        {
            GUIUtility.systemCopyBuffer = data;
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
            RectTransform rect = (RectTransform) trigger.transform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, trigger.camera, out Vector2 offset);
            offset -= new Vector2(rect.sizeDelta.x / 2, 3f);

            UIButtonTip buttonTip = tipObject as UIButtonTip;
            if (buttonTip == null)
            {
                buttonTip = UIButtonTip.Create(false, "Copy", "Click to copy to clipboard", Corner, offset, 0, rect, "", "");
                if (tipObject != null)
                {
                    Object.Destroy(tipObject.gameObject);
                }

                tipObject = buttonTip;
            }

            if (!buttonTip.gameObject.activeSelf)
            {
                buttonTip.gameObject.SetActive(true);
                buttonTip.SetTip(false, "Copy", "Click to copy to clipboard", Corner, offset, 0, rect, "", "");
            }

            if (buttonTip != null && buttonTip.isActiveAndEnabled && !buttonTip.titleComp.Equals("Copy"))
            {
                buttonTip.SetTip(false, "Copy", "Click to copy to clipboard", Corner, offset, 0, rect, "", "");
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

        public static string FormatCopyString(string data, bool isSensitive = false, Func<string, string> filter = null)
        {
            if (isSensitive && NebulaModel.Config.Options.StreamerMode)
            {
                string safeText = filter != null ? filter(data) : new string('*', data.Length);
                return $"<link=\"copytext {data}\"><color=\"blue\"><u>{safeText}</u></color></link>";
            }
            return $"<link=\"copytext {data}\"><color=\"blue\"><u>{data}</u></color></link>";
        }
    }
}