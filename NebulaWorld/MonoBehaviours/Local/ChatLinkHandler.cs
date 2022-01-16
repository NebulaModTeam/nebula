using NebulaModel.Logger;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NebulaWorld.MonoBehaviours.Local
{
    public class ChatLinkHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Camera cam;
        public TMP_Text text;

        public bool isPointerInside;
        public string currentLinkID;

        private void Awake()
        {
            text = GetComponent<TMP_Text>();
        }

        private void Update()
        {
            if (isPointerInside)
            {
                Vector3 mouse = Input.mousePosition;
                int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, mouse, cam); 
                // If you are not in a Canvas using Screen Overlay, put your camera instead of null
            
                if (linkIndex != -1)
                {
                    TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
                    currentLinkID = linkInfo.GetLinkID();
                }

                if (Input.GetMouseButtonDown(0))
                {
                    Log.Info($"User clicked on link ID: {currentLinkID}");
                }
            }
            
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isPointerInside = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerInside = false;
        }
    }
}