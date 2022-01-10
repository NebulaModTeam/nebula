using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class AssetRiddanceHelper : MonoBehaviour
{
    [MenuItem("Window/DSP Tools/Get rid of assets")]
    public static void Riddance()
    {
        GameObject gameObject = Selection.activeGameObject;
            
        if (gameObject == null)
        {
            Debug.Log("You have nothing selected!");
            return;
        }

        Sprite defSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0,0,4,4), new Vector2(0.5f,0.5f));
        Font defFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        
        Image[] images = gameObject.GetComponentsInChildren<Image>();

        foreach (Image image in images)
        {
            if (image.sprite != null)
            {
                string name = image.sprite.name;

                AssetMemory memory = image.gameObject.AddComponent<AssetMemory>();

                memory.assetName = name;
                memory.assetType = nameof(Sprite);
                image.sprite = defSprite;
                EditorUtility.SetDirty(image);
            }
        } 
        
        Text[] texts = gameObject.GetComponentsInChildren<Text>();

        foreach (Text text in texts)
        {
            if (text.font != null)
            {
                string name = text.font.name;

                AssetMemory memory = text.gameObject.AddComponent<AssetMemory>();

                memory.assetName = name;
                memory.assetType = nameof(Font);
                text.font = defFont;
                EditorUtility.SetDirty(text);
            }
        } 
    }
    
    [MenuItem("Window/DSP Tools/Reassign assets")]
    public static void Reassign()
    {
        GameObject gameObject = Selection.activeGameObject;
            
        if (gameObject == null)
        {
            Debug.Log("You have nothing selected!");
            return;
        }
        
        Image[] images = gameObject.GetComponentsInChildren<Image>();

        foreach (Image image in images)
        {
            AssetMemory memory = image.GetComponent<AssetMemory>();
            if (memory != null && !memory.assetName.Equals("") && memory.assetType == nameof(Sprite))
            {
                string[] found = AssetDatabase.FindAssets(memory.assetName + " t:sprite");
                foreach (string path in found)
                {
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(path));
                    if (sprite.name.Equals(memory.assetName))
                    {
                        image.sprite = sprite;
                        DestroyImmediate(memory);
                        EditorUtility.SetDirty(image.gameObject);

                        break;
                    }
                }
            }
        }
        
        Text[] texts = gameObject.GetComponentsInChildren<Text>();

        foreach (Text text in texts)
        {
            AssetMemory memory = text.GetComponent<AssetMemory>();
            if (memory != null && !memory.assetName.Equals("") && memory.assetType == nameof(Font))
            {
                string[] found = AssetDatabase.FindAssets(memory.assetName + " t:font");
                foreach (string path in found)
                {
                    Font font = AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(path));
                    if (font.name.Equals(memory.assetName))
                    {
                        text.font = font;
                        DestroyImmediate(memory);
                        EditorUtility.SetDirty(text.gameObject);

                        break;
                    }
                }
            }
        }
    }
}
