using NebulaModel.Logger;
using UnityEngine;

namespace NebulaWorld.Utils
{
    public static class UnityUtils
    {
        public static void PrintObjectStructure(GameObject obj, int indentation = 0)
        {
            if (obj == null)
                return;
            string indent = "";
            for (int i = 0; i < indentation; i++)
                indent += '\t';

            Log.Info(indent + obj.name);
            foreach (Component c in obj.GetComponents<Component>())
            {
                if (c != null && c.GetType() != typeof(Transform))
                    Log.Info("\t" + indent + "Component: " + c.GetType().Name.ToString());
            }

            for (int i = 0; i < obj.transform.childCount; i++)
            {
                GameObject child = obj.transform.GetChild(i).gameObject;
                if (child != null)
                    PrintObjectStructure(child, indentation + 1);
            }
        }
    }
}
