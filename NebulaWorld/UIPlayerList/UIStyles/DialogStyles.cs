using UnityEngine;

namespace NebulaWorld.UIPlayerList.UIStyles
{
    internal class DialogStyles
    {
        // ReSharper disable once InconsistentNaming
        private static Texture2D _textureMemory = null;

        public static GUIStyle WindowBackgroundStyle()
        {
            var width = 32;
            var height = 32;

            if (_textureMemory == null)
                _textureMemory = MakeTex(width, height, new Color32(36, 67, 76, 200));

            var style = new GUIStyle(GUI.skin.box)
            {
                normal = new GUIStyleState()
                {
                    background = _textureMemory
                },
            };

            return style;
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pixels);
            result.Apply();
            return result;
        }
    }
}
