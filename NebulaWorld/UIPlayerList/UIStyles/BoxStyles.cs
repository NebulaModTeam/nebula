using UnityEngine;

namespace NebulaWorld.UIPlayerList.UIStyles
{
    internal class BoxStyles
    {
        public static GUILayoutOption[] HorizontalSliderStyle => new GUILayoutOption[]
        {
            GUILayout.ExpandWidth(true), GUILayout.Height(1), GUILayout.MaxHeight(1)
        };
    }
}
