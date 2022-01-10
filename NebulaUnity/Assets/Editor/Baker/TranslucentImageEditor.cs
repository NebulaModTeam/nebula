using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(TranslucentImage))]
public class TranslucentImageEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        /*TranslucentImage trg = (TranslucentImage) target;
            
        var serializedObject = new SerializedObject(trg);
        var property = serializedObject.FindProperty("source");
        serializedObject.Update();
        EditorGUILayout.PropertyField(property, true);
        serializedObject.ApplyModifiedProperties();
        */
    }
}
#endif
