using UnityEngine;

public static class TransformUtils
{
    // Helper function that gets all children of a hierarchy
    public static int CountAllChildren(this Transform parentObject)
    {
        return GetChildrenRecursive(parentObject);
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
