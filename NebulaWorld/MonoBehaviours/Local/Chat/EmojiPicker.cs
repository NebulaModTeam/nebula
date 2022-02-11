using NebulaWorld.Chat;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NebulaWorld.MonoBehaviours.Local
{
    public class EmojiPicker : MonoBehaviour
    {
        [SerializeField] private TMP_Text selectText;
        [SerializeField] private RectTransform selectionDisplay;
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private RectTransform selectorContentTrans;
        [SerializeField] private RawImage contentImage;

        [SerializeField] private Material contentMat;
        [SerializeField] private TextAsset emojiJson;
        

        private uint[] emojiDatas;
        internal ComputeBuffer EmojiBuffer;
        
        private static readonly int buffer = Shader.PropertyToID("_EmojiBuffer");
        private static readonly int grid = Shader.PropertyToID("_Grid");

        public string currentCategory = "Smileys & Emotion";
        

        private static string[] categoryNames =
        {
            "Smileys & Emotion",
            "Animals & Nature",
            "Food & Drink",
            "Activities",
            "Travel & Places",
            "Objects",
            "Symbols",
            "Flags"
        };

        private void Awake()
        {
            emojiDatas = new uint[1024];
            EmojiBuffer = new ComputeBuffer(emojiDatas.Length, 4);
            EmojiBuffer.SetData(emojiDatas);
            contentImage.material = contentMat;
            contentImage.materialForRendering.SetBuffer(buffer, EmojiBuffer);
            contentImage.SetMaterialDirty();
            
            EmojiDataManager.ParseData(emojiJson);
            RefreshIcons();
        }

        public void SetCurrentSelection(int index)
        {
            float selPosition = 10.5f + 50 * index;
            selectionDisplay.anchoredPosition = new Vector2(0, -selPosition);
            currentCategory = categoryNames[index];
            RefreshIcons();
        }

        private void OnDestroy()
        {
            emojiDatas = null;
            EmojiBuffer.Dispose();
        }

        public void ClearEmojiDisplay()
        {
            for (int index = 0; index < emojiDatas.Length; index++)
            {
                emojiDatas[index] = 0;
            }
            EmojiBuffer.SetData(emojiDatas);
        }

        private void RefreshIcons()
        {
            ClearEmojiDisplay();
            if (EmojiDataManager.emojies.ContainsKey(currentCategory))
            {
                List<Emoji> emojis = EmojiDataManager.emojies[currentCategory];
                for (int i = 0; i < emojis.Count; i++)
                {
                    Emoji emoji = emojis[i];
                    uint index = (uint) (emoji.SheetX + emoji.SheetY * 62 + 1);
                    emojiDatas[i] = index;
                }

                int newVSize = Mathf.CeilToInt(emojis.Count / 8f);
                
                contentImage.materialForRendering.SetVector(grid, new Vector4(8, newVSize));
                EmojiBuffer.SetData(emojiDatas);
                contentImage.materialForRendering.SetBuffer(buffer, EmojiBuffer);
                
                selectorContentTrans.sizeDelta = new Vector2(selectorContentTrans.sizeDelta.x, 40 * newVSize);
                contentImage.SetMaterialDirty();
            }
        }
    }
}