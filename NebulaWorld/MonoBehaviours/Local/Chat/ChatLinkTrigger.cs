using NebulaWorld.Chat;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NebulaWorld.Chat
{
    public class ChatLinkTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public const int HoverDelay = 1;
        
        public Camera camera;
        public TMP_Text text;

        public string currentLink;

        private bool isPointerInside;
        private float insideTime;
        private static MonoBehaviour tip;
        
        
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
                if (currentLink != linkInfo.GetLinkID())
                {
                    currentLink = linkInfo.GetLinkID();
                    insideTime = 0;
                    CloseTips();
                }

                string linkID = RichChatLinkRegistry.ParseRichText(currentLink, out string linkData);
                IChatLinkHandler handler = RichChatLinkRegistry.GetChatLinkHandler(linkID);

                if (handler != null)
                {
                    if (insideTime > HoverDelay)
                    {
                        handler.OnHover(linkData, this, ref tip);
                    }
                    
                    if (Input.GetMouseButtonDown(0))
                    {
                        handler.OnClick(linkData);
                    }
                }
            }
            else
            {
                currentLink = "";
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
            CloseTips();
        }

        public static void CloseTips()
        {
            if (tip == null) return;

            Destroy(tip.gameObject);
        }
    }
}