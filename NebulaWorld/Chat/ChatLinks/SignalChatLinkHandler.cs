using NebulaModel.Utils;
using NebulaWorld.MonoBehaviours.Local;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NebulaWorld.Chat
{
    public class SignalChatLinkHandler : IChatLinkHandler
    {
        public const int Corner = 2;

        public static string GetLinkString(int signalId)
        {
            return $"signal {signalId}";
        }
        
        public void OnClick(string data)
        {
            
        }

        public void OnHover(string data, ChatLinkTrigger trigger, ref MonoBehaviour tipObject)
        {
            int signalId = GetSignalId(data);
            if (signalId <= 0) return;

            if (signalId < 1000 || signalId > 20000)
            {
                if (tipObject != null)
                {
                    Object.Destroy(tipObject.gameObject);
                }
                return;
            }

            RectTransform rect = (RectTransform)trigger.transform;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, trigger.camera, out Vector2 offset);
            offset -= new Vector2(rect.sizeDelta.x / 2, 1.5f);
            
            UIItemTip uiitemTip = tipObject as UIItemTip;
            if (uiitemTip == null)
            {
                uiitemTip = UIItemTip.Create(signalId, Corner, offset, trigger.transform, 1, 0, UIButton.ItemTipType.Item);
                if (tipObject != null)
                {
                    Object.Destroy(tipObject.gameObject);
                }
                tipObject = uiitemTip;
            }
            if (!uiitemTip.gameObject.activeSelf)
            {
                uiitemTip.gameObject.SetActive(true);
                uiitemTip.SetTip(signalId, Corner, offset, trigger.transform, 1, 0, UIButton.ItemTipType.Item);
            }
            if (uiitemTip != null && uiitemTip.isActiveAndEnabled && uiitemTip.showingItemId != signalId)
            {
                uiitemTip.SetTip(signalId, Corner, offset, trigger.transform, 1, 0, UIButton.ItemTipType.Item);
            }
        }

        public string GetIconName(string data)
        {
            int signalId = GetSignalId(data);
            return signalId <= 0 ? "Unknown" : signalId.ToString();
        }

        public string GetDisplayRichText(string data)
        {
            int signalId = GetSignalId(data);
            if (signalId <= 0) return "";

            return $"<link=\"signal {signalId}\">[<sprite name=\"{signalId}\"> <color=\"green\">{ProtoUtils.GetSignalDisplayName(signalId)}</color>]</link>";
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
}