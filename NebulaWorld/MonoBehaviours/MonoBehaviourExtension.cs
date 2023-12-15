#region

using UnityEngine;

#endregion

namespace NebulaWorld.MonoBehaviours;

public static class MonoBehaviourExtension
{
    private static Camera canvasCamera;

    public static T AddComponentIfMissing<T>(this GameObject go) where T : Component
    {
        var component = go.GetComponent<T>();
        if (component == null)
        {
            component = go.AddComponent<T>();
        }
        return component;
    }

    public static bool MouseInRect(this RectTransform rectTransform)
    {
        if (canvasCamera == null)
        {
            canvasCamera = UIRoot.instance.overlayCanvas.worldCamera;
        }

        Vector2 mousePos = Input.mousePosition;
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePos, canvasCamera);
    }
}
