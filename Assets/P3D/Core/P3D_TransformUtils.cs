using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace pricenerds3D
{
    /// <summary>
    /// Helper utilities for unity transforms
    /// </summary>
    public static class TransformUtils
    {
        /// <summary>
        /// Gets total amount of children in parentObject recursively
        /// </summary>
        /// <param name="parentObject"></param>
        /// <returns></returns>
        public static int CountAllChildren(this Transform parentObject)
        {
            return ChildrenCountRecursive(parentObject);
        }

        /// <summary>
        /// Gets all children in a transform
        /// </summary>
        /// <param name="parentObject"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the number
        /// </summary>
        /// <param name="parentObject"></param>
        /// <returns></returns>
        private static int ChildrenCountRecursive(Transform parentObject)
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
