using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pricenerds3D
{
    public static class TransformUtils
    {
        // Helper function that gets all children of a hierarchy
        public static int CountAllChildren(this Transform parentObject)
        {
            return GetChildrenRecursive(parentObject);
        }

        public static List<Transform> GetAllChildren(this Transform parentObject)
        {
            List<Transform> list = new List<Transform>();

            for (int i = 0; i < parentObject.childCount; i++)
            {
                Transform child = parentObject.GetChild(i);
                list.Add(child);

                if (child.childCount > 0)
                {
                    list.AddRange(GetAllChildren(child));
                }
            }

            return list;
        }

        private static int GetChildrenRecursive(Transform parentObject)
        {
            int sum = 0;

            for (int i = 0; i < parentObject.childCount; i++)
            {
                Transform child = parentObject.GetChild(i);
                sum++;

                if (child.childCount > 0)
                {
                    sum += CountAllChildren(child);
                }
            }

            return sum;
        }
    }
}
