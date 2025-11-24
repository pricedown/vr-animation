using UnityEngine;

namespace pricenerds3D
{
    public static class P3D_SkeletonHelpers
    {
        // notes for joe:
        // this is a helper function I wrote for drawing a bone from a start to end 
        // it draws a bone similar to how a program like blender / maya would
        // you call it in a recursive loop to draw all bones of a skeletal hierarchy
        public static void DrawBoneGizmo(Vector3 start, Vector3 end)
        {
            Vector3 dir = end - start;
            float length = dir.magnitude;

            // avoid division by zero errors
            if (length == 0.0f) return;
            dir /= length;

            // create an orthonormal basis
            Vector3 zOrtho = dir;

            // if we're super lined up with the axis it might not render anything
            Vector3 xOrtho = Vector3.Cross(zOrtho, Vector3.right).normalized;
            if (xOrtho.magnitude < 0.001f) xOrtho = Vector3.Cross(zOrtho, Vector3.up).normalized;

            Vector3 yOrtho = Vector3.Cross(zOrtho, xOrtho);

            // this builds a transformation matrix that we can use fo
            Matrix4x4 transformMat = new Matrix4x4();
            transformMat.SetColumn(0, new Vector4(xOrtho.x, xOrtho.y, xOrtho.z, 0));
            transformMat.SetColumn(1, new Vector4(yOrtho.x, yOrtho.y, yOrtho.z, 0));
            transformMat.SetColumn(2, new Vector4(zOrtho.x, zOrtho.y, zOrtho.z, 0));
            transformMat.SetColumn(3, new Vector4(start.x, start.y, start.z, 1));

            float baseWidth = length * 0.1f;
            float topLength = length * 0.9f;
            float bottomLength = length * 0.1f;

            // get the top and bottom of the bone
            Vector3 top = transformMat.MultiplyPoint3x4(new Vector3(0, 0, topLength + bottomLength));
            Vector3 bottom = transformMat.MultiplyPoint3x4(new Vector3(0, 0, 0));

            // get each point using the base width to form the rectangular base of the pyramid 
            Vector3 b1 = transformMat.MultiplyPoint3x4(new Vector3(+baseWidth, +baseWidth, bottomLength));
            Vector3 b2 = transformMat.MultiplyPoint3x4(new Vector3(+baseWidth, -baseWidth, bottomLength));
            Vector3 b3 = transformMat.MultiplyPoint3x4(new Vector3(-baseWidth, -baseWidth, bottomLength));
            Vector3 b4 = transformMat.MultiplyPoint3x4(new Vector3(-baseWidth, +baseWidth, bottomLength));

            // connect the base to the top
            Gizmos.DrawLine(b1, top);
            Gizmos.DrawLine(b2, top);
            Gizmos.DrawLine(b3, top);
            Gizmos.DrawLine(b4, top);

            // connect the base to form a rectangle
            Gizmos.DrawLine(b1, b2);
            Gizmos.DrawLine(b2, b3);
            Gizmos.DrawLine(b3, b4);
            Gizmos.DrawLine(b4, b1);

            // connect the base to the bottom
            Gizmos.DrawLine(b1, bottom);
            Gizmos.DrawLine(b2, bottom);
            Gizmos.DrawLine(b3, bottom);
            Gizmos.DrawLine(b4, bottom);
        }

        // draws a skeleton by iterating through all joints of the skeleton
        public static void DrawSkeletonPoseGizmo(P3D_SkeletonPose pose) // note for later, pass in P3D_SkeletonPose, not P3D_Skeleton
        {
            for (int i = 0; i < pose.m_skeleton.m_jointCount; i++)
            {
                sbyte parentIndex = pose.m_skeleton.m_joints[i].m_parentIndex;

                if (parentIndex == -1) continue;

                // NOTES for joe!!
                // MultiplyPoint3x4 is a cool function, it's optimized for what we want!!!
                // we will only ever make affine transformations, which is what MultiplyPoint3x4 accels at
                // the function wipes out the last row which is always constant [0 0 0 1]
                // MultiplyPoint3x4 also can be used to just quickly extract the translation component because you can just zero out the rotation and scale properties to get translation back

                Vector3 start = pose.m_worldSpace[parentIndex].MultiplyPoint3x4(Vector3.zero);
                Vector3 end = pose.m_worldSpace[i].MultiplyPoint3x4(Vector3.zero);
                DrawBoneGizmo(start, end);
            }
        }

        // This function is responsible for recursively creating a skeleton from a Transform hierarchy
        public static void CreateSkeletonFromHierarchy(Transform hierarchyParent, P3D_Skeleton skeleton)
        {
            sbyte currentIndex = 0;

            CreateSkeletonFromHierarchyRecursive(hierarchyParent.GetChild(0), ref currentIndex, -1, skeleton);
        }

        // Helper function that manages the recursion of hierarchy creation
        private static void CreateSkeletonFromHierarchyRecursive(Transform current, ref sbyte currentIndex, sbyte parentIndex, P3D_Skeleton skeleton)
        {
            // keep track of the current index to send to children to use as the parentIndex
            sbyte self = currentIndex;
            skeleton.m_joints[self] = CreateJointByGameObject(current, parentIndex);

            // keeps track of where we are in the array
            currentIndex++;

            // Recursive step to continue filling out the skeleton
            for (int i = 0; i < current.childCount; i++)
            {
                Transform child = current.GetChild(i);
                CreateSkeletonFromHierarchyRecursive(child, ref currentIndex, self, skeleton);
            }
        }

        // Helper function that creates a joint based on a passed GameObject
        public static P3D_Joint CreateJointByGameObject(Transform transform, sbyte parentIndex)
        {
            P3D_Joint joint = new P3D_Joint();

            joint.m_name = transform.name;
            joint.m_parentIndex = parentIndex;
            joint.m_localPosition = transform.transform.localPosition;
            joint.m_localRotation = transform.transform.localRotation;
            joint.m_localScale = transform.transform.localScale;

            return joint;
        }
    }
}