using UnityEngine;

namespace NebulaWorld.UIPlayerList.UIStyles
{
    internal class LabelStyles
    {
        public static GUIStyle HeaderLabelStyle = new GUIStyle(UnityEngine.GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 24,
            fontStyle = FontStyle.Bold
        };

        public static GUIStyle CenterLabelLarge = new GUIStyle(UnityEngine.GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 42,
            fontStyle = FontStyle.Bold
        };

        public static GUIStyle RowHeaderLabelsStyle = new GUIStyle(UnityEngine.GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 18,
            fontStyle = FontStyle.Bold
        };

        public static GUIStyle RowLabelStyle = new GUIStyle(UnityEngine.GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 18
        };
    }
}
