#region

using NebulaWorld.Chat;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

#endregion

namespace NebulaWorld.MonoBehaviours.Local.Chat;

public class ChatLinkTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private const int HoverDelay = 1;
    private static MonoBehaviour tip;

    public Camera camera;
    public TMP_Text text;

    public string currentLink;
    private float insideTime;

    private bool isPointerInside;


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
        if (!isPointerInside)
        {
            return;
        }

        insideTime += Time.deltaTime;

        var mouse = Input.mousePosition;
        var linkIndex = TMP_TextUtilities.FindIntersectingLink(text, mouse, camera);

        if (linkIndex != -1)
        {
            var linkInfo = text.textInfo.linkInfo[linkIndex];
            if (currentLink != linkInfo.GetLinkID())
            {
                currentLink = linkInfo.GetLinkID();
                insideTime = 0;
                CloseTips();
            }

            var linkID = RichChatLinkRegistry.ParseRichText(currentLink, out var linkData);
            var handler = RichChatLinkRegistry.GetChatLinkHandler(linkID);

            if (handler == null)
            {
                return;
            }
            if (insideTime > HoverDelay)
            {
                handler.OnHover(linkData, this, ref tip);
            }

            if (Input.GetMouseButtonDown(0))
            {
                handler.OnClick(linkData);
            }
            if (Input.GetMouseButtonDown(1))
            {
                handler.OnRightClick(linkData);
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
        if (tip == null)
        {
            return;
        }

        Destroy(tip.gameObject);
    }
}
