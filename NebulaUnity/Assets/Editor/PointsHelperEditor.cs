using System.Collections.Generic;
using CommonAPI;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
    [CustomEditor(typeof(PointsHelper))]
    public class PointsHelperEditor : Editor
    {
        public int selected = 0;
        
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Please select root of the prefab and type of points.");
        
            base.OnInspectorGUI();
            PointsHelper trg = (PointsHelper) target;

            if (trg.target != null && trg.pointsType == PointsType.Custom && trg.target.GetType() == typeof(Transform))
            {
                IPointsAssignable[] components = trg.target.gameObject.GetComponents<IPointsAssignable>();

                List<string> options = new List<string>(components.Length + 1);
                options.Add("Ambiguous target. Please select");
                string lastOption = "";
                int count = 0;
                foreach (IPointsAssignable component in components)
                {
                    if (component.GetType().Name.Equals(lastOption))
                    {
                        count++;
                        options.Add(component.GetType().Name + $" ({count})");
                    }
                    else
                    {
                        lastOption = component.GetType().Name;
                        count = 0;
                        options.Add(lastOption);
                    }
                }
                
                selected = EditorGUILayout.Popup(selected, options.ToArray());
                if (selected > 0 && selected - 1 < components.Length)
                {
                    trg.target = (Component) components[selected - 1];
                }

            }
            
            if (GUILayout.Button("Apply points"))
            {
                trg.Assign();
            }

        }
    }
#endif
