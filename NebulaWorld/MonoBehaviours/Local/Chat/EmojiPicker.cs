#region

using System;
using System.Collections.Generic;
using System.Linq;
using NebulaModel.Utils;
using NebulaWorld.Chat;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#endregion

namespace NebulaWorld.MonoBehaviours.Local.Chat;

public class EmojiPicker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static EmojiPicker instance;
    private static Action<Emoji> callback;


    private static readonly int buffer = Shader.PropertyToID("_EmojiBuffer");
    private static readonly int grid = Shader.PropertyToID("_Grid");


    private static readonly string[] categoryNames =
    {
        "Smileys & Emotion", "Animals & Nature", "Food & Drink", "Activities", "Travel & Places", "Objects", "Symbols",
        "Flags"
    };

    [SerializeField] private TMP_Text selectText;
    [SerializeField] private RectTransform selectionDisplay;
    [SerializeField] private TMP_InputField searchField;
    [SerializeField] private RectTransform selectorContentTrans;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RawImage contentImage;
    [SerializeField] private RectTransform inspectTrans;

    [SerializeField] private Material contentMat;
    [SerializeField] private TextAsset emojiJson;

    public bool pointerIn;
    private string currentCategory = "Smileys & Emotion";

    private int currentSelection;
    private int currentSize;
    private ComputeBuffer emojiBuffer;
    private uint[] emojiDatas;
    private string lastSearch = "";

    private List<Emoji> lastUsedList;

    public void Awake()
    {
        instance = this;
        emojiDatas = new uint[1024];
        emojiBuffer = new ComputeBuffer(emojiDatas.Length, 4);
        emojiBuffer.SetData(emojiDatas);
        contentImage.material = contentMat;
        contentImage.materialForRendering.SetBuffer(buffer, emojiBuffer);
        contentImage.SetMaterialDirty();

        EmojiDataManager.ParseData(emojiJson);
        RefreshIcons(lastSearch);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        UpdateRaycast();
        UpdateClick();

        emojiBuffer.SetData(emojiDatas);
        contentImage.materialForRendering.SetBuffer(buffer, emojiBuffer);
        contentImage.SetMaterialDirty();
    }

    private void OnEnable()
    {
        RefreshIcons(lastSearch);
    }

    private void OnDestroy()
    {
        emojiDatas = null;
        emojiBuffer.Dispose();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerIn = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerIn = false;
    }

    public static void Open(Action<Emoji> callback)
    {
        instance.gameObject.SetActive(true);
        EmojiPicker.callback = callback;
    }

    public static void Close()
    {
        callback = null;
        instance.gameObject.SetActive(false);
        instance.pointerIn = false;
        instance.lastSearch = "";
        instance.searchField.SetTextWithoutNotify("");
    }

    public static bool IsOpen()
    {
        return instance.gameObject.activeSelf;
    }

    public void SetCurrentSelection(int index)
    {
        var selPosition = 10.5f + 50 * index;
        selectionDisplay.anchoredPosition = new Vector2(0, -selPosition);
        currentCategory = categoryNames[index];
        RefreshIcons(lastSearch);
    }

    public void SearchEmoji(string input)
    {
        RefreshIcons(input);
        lastSearch = input;
    }

    public void ClearEmojiDisplay()
    {
        for (var index = 0; index < emojiDatas.Length; index++)
        {
            emojiDatas[index] = 0;
        }
        emojiBuffer.SetData(emojiDatas);
    }

    private void UpdateClick()
    {
        if (currentSelection == -1)
        {
            return;
        }
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        if (currentSelection >= lastUsedList.Count)
        {
            return;
        }

        var emoji = lastUsedList[currentSelection];
        callback?.Invoke(emoji);
        VFInput.UseMouseLeft();
        Close();
    }

    private void UpdateRaycast()
    {
        if (!scrollRect.viewport.MouseInRect())
        {
            HideCursor();
            return;
        }

        if (UIRoot.ScreenPointIntoRect(Input.mousePosition, selectorContentTrans, out var rectPoint))
        {
            var x = Mathf.FloorToInt(rectPoint.x / 40) + 4;
            var y = Mathf.FloorToInt(-rectPoint.y / 40);

            if (y < 0 || y >= currentSize || x < 0 || x >= 8)
            {
                HideCursor();
                return;
            }

            var index = x + y * 8;

            if (index < lastUsedList.Count)
            {
                var emoji = lastUsedList[index];
                selectText.text = emoji.ShortName;

                currentSelection = index;
                inspectTrans.anchoredPosition = new Vector2(40 * x - 2, -40 * y - 2);
                inspectTrans.gameObject.SetActive(true);
                return;
            }
        }

        HideCursor();
    }

    private void HideCursor()
    {
        currentSelection = -1;
        inspectTrans.gameObject.SetActive(false);
    }

    private void RefreshIcons(string search)
    {
        ClearEmojiDisplay();
        if (EmojiDataManager.emojies.TryGetValue(currentCategory, out var value))
        {
            if (search.Equals(""))
            {
                lastUsedList = value;
            }
            else
            {
                lastUsedList = value;
                lastUsedList = lastUsedList.Where(emoji => emoji.ShortName.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            for (var i = 0; i < lastUsedList.Count; i++)
            {
                var emoji = lastUsedList[i];
                var index = (uint)(emoji.SheetX + emoji.SheetY * 62 + 1);
                emojiDatas[i] = index;
            }

            currentSize = Mathf.CeilToInt(lastUsedList.Count / 8f);

            contentImage.materialForRendering.SetVector(grid, new Vector4(8, currentSize));
            emojiBuffer.SetData(emojiDatas);
            contentImage.materialForRendering.SetBuffer(buffer, emojiBuffer);

            scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, 40 * currentSize + 8);
            contentImage.SetMaterialDirty();
        }
        else
        {
            currentSize = 0;
        }
    }
}
