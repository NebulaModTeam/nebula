using NebulaModel.Logger;
using NebulaWorld.Chat;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NebulaWorld.MonoBehaviours.Local
{
    public class ChatLinkTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public const int HoverDelay = 1;
        
        public Camera camera;
        public TMP_Text text;

        public string currentLinkID;

        private bool isPointerInside;
        private float insideTime;
        
        private static Dictionary<string, IChatLinkHandler> handlers = new Dictionary<string, IChatLinkHandler>();
        private static MonoBehaviour tip;

        public static void RegisterChatLinkHandler(string linkID, IChatLinkHandler handler)
        {
            if (handler == null) return;
            if (handlers.ContainsKey(linkID))
            {
                Log.Debug($"Can't register handler, because handler for {linkID} was already registered!");
                return;
            }
            
            Log.Debug($"Registering Chat Link handler for {linkID}");
            handlers.Add(linkID, handler);
        }

        static ChatLinkTrigger()
        {
            RegisterChatLinkHandler("signal", new SignalChatLinkHandler());
        }
        
        
        
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
                    CloseTips();
                }
                
                // ChatLink ID is always "handlerName data"
                string[] splitStrings = currentLinkID.Split(' ');
                if (splitStrings.Length != 2) return;

                if (handlers.ContainsKey(splitStrings[0]))
                {
                    IChatLinkHandler handler = handlers[splitStrings[0]];
                    if (insideTime > HoverDelay)
                    {
                        handler.OnHover(splitStrings[1], this, ref tip);
                    }
                    
                    if (Input.GetMouseButtonDown(0))
                    {
                        handler.OnClick(splitStrings[1]);
                    }
                }
            }
            else
            {
                currentLinkID = "";
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