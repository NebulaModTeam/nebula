using NebulaWorld.MonoBehaviours.Local;
using UnityEngine;
using UnityEngine.UI;

namespace NebulaWorld.Chat
{
    public class SignalChatLinkHandler : IChatLinkHandler
    {
        public const int Corner = 2;
        
        public void OnClick(string tipData)
        {
            
        }

        public void OnHover(string tipData, ChatLinkTrigger trigger, ref MonoBehaviour tipObject)
        {
            int signalId = int.Parse(tipData);
            
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
            offset -= new Vector2(rect.sizeDelta.x / 2, 0);
            
            UIItemTip uiitemTip = tipObject as UIItemTip;
            if (uiitemTip == null)
            {
                uiitemTip = UIItemTip.Create(signalId, Corner, offset, trigger.transform);
                if (tipObject != null)
                {
                    Object.Destroy(tipObject.gameObject);
                }
                tipObject = uiitemTip;
            }
            if (!uiitemTip.gameObject.activeSelf)
            {
                uiitemTip.gameObject.SetActive(true);
                uiitemTip.SetTip(signalId, Corner, offset, trigger.transform);
            }
            if (uiitemTip != null && uiitemTip.isActiveAndEnabled && uiitemTip.showingItemId != signalId)
            {
                uiitemTip.SetTip(signalId, Corner, offset, trigger.transform);
            }
        }
    }
}