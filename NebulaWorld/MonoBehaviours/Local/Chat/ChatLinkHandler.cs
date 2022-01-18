using NebulaModel.Logger;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NebulaWorld.MonoBehaviours.Local
{
    public class ChatLinkHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public const int Corner = 2;
        public const int HoverDelay = 1;
        
        public Camera camera;
        public TMP_Text text;
        
        private static MonoBehaviour tip;

        public bool isPointerInside;
        public float insideTime;
        
        public string currentLinkID;

        private void Awake()
        {
            text = GetComponent<TMP_Text>();
            if (UIRoot.instance != null)
            {
                camera = UIRoot.instance.overlayCanvas.worldCamera;
            }
        }

        private void Update()
        {
            if (!isPointerInside) return;

            insideTime += Time.deltaTime;
                
            Vector3 mouse = Input.mousePosition;
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, mouse, camera); 
            
            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
                if (currentLinkID != linkInfo.GetLinkID())
                {
                    currentLinkID = linkInfo.GetLinkID();
                    insideTime = 0;
                    if (tip != null)
                    {
                        Destroy(tip.gameObject);
                    }
                }

                if (insideTime > HoverDelay)
                {
                    HandleTips(currentLinkID);
                }
                    
                if (Input.GetMouseButtonDown(0))
                {
                    Log.Info($"User clicked on link ID: {currentLinkID}");
                }
            }
            else
            {
                currentLinkID = "";
            }
        }

        private void HandleTips(string tipID)
        {
            string[] splitStrings = tipID.Split(' ');
            if (splitStrings.Length != 2) return;
            
            switch (splitStrings[0])
            {
                case "signal":
                    int signalId = int.Parse(splitStrings[1]);
                    CreateTip(signalId);
                    break;
            }
        }

        private void CreateTip(int signalId)
        {
            if (signalId < 1000 || signalId > 20000)
            {
                if (tip != null)
                {
                    Destroy(tip.gameObject);
                }
                return;
            }

            RectTransform rect = (RectTransform)transform;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition,camera, out Vector2 offset);
            offset -= new Vector2(rect.sizeDelta.x / 2, 0);
            
            UIItemTip uiitemTip = tip as UIItemTip;
            if (uiitemTip == null)
            {
                uiitemTip = UIItemTip.Create(signalId, Corner, offset, transform);
                if (tip != null)
                {
                    Destroy(tip.gameObject);
                }
                tip = uiitemTip;
            }
            if (!uiitemTip.gameObject.activeSelf)
            {
                uiitemTip.gameObject.SetActive(true);
                uiitemTip.SetTip(signalId, Corner, offset, transform);
            }
            if (uiitemTip != null && uiitemTip.isActiveAndEnabled && uiitemTip.showingItemId != signalId)
            {
                uiitemTip.SetTip(signalId, Corner, offset, transform);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isPointerInside = true;
            insideTime = 0;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerInside = false;
            insideTime = 0;
            if (tip != null)
            {
                Destroy(tip.gameObject);
            }
        }

        public static void CloseTips()
        {
            if (tip != null)
            {
                Destroy(tip.gameObject);
            }
        }
    }
}