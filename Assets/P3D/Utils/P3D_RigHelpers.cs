using UnityEngine;

// Written by Seth Riddensdale
namespace pricenerds3D
{
    public static class P3D_RigHelpers
    {
        #region Gizmos
        /// <summary>
        /// This is a static helper function draws a gizmo representing a single bone from provided start and end world space positions
        /// You can use DrawRigPoseGizmo() to draw the entire Rig
        /// </summary>
        public static void DrawBoneGizmo(Vector3 start, Vector3 end, P3D_RigPose pose, int jointIndex)
        {
            Vector3 dir = end - start;
            float length = dir.magnitude;

            // avoid division by zero errors
            if (length == 0.0f) return;
            dir /= length;

            /*
            // create an orthonormal basis
            Vector3 zOrtho = dir;
            // if we're super lined up with the axis it might not render anything
            Vector3 xOrtho = Vector3.Cross(Vector3.right, zOrtho).normalized;
            if (xOrtho.magnitude < 0.001f) xOrtho = Vector3.Cross(Vector3.up, zOrtho).normalized;
            Vector3 yOrtho = Vector3.Cross(zOrtho, xOrtho).normalized;

            Matrix4x4 transformMat = new Matrix4x4();
            transformMat.SetColumn(0, new Vector4(xOrtho.x, xOrtho.y, xOrtho.z, 0));
            transformMat.SetColumn(1, new Vector4(yOrtho.x, yOrtho.y, yOrtho.z, 0));
            transformMat.SetColumn(2, new Vector4(zOrtho.x, zOrtho.y, zOrtho.z, 0));
            transformMat.SetColumn(3, new Vector4(start.x, start.y, start.z, 1));*/

            Vector3 xOrtho = pose.m_localPose[jointIndex].m_jointRight;
            Vector3 yOrtho = pose.m_localPose[jointIndex].m_jointUp;
            Vector3 zOrtho = pose.m_localPose[jointIndex].m_jointForward;

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

            Debug.DrawRay(bottom, xOrtho * 0.05f, Color.blue);
            Debug.DrawRay(bottom, zOrtho * 0.05f, Color.green);
            Debug.DrawRay(bottom, yOrtho * 0.05f, Color.red);

            // get each point using the base width to form the rectangular base of the pyramid 
            Vector3 b1 = transformMat.MultiplyPoint3x4(new Vector3(+baseWidth, +baseWidth, bottomLength));
            Vector3 b2 = transformMat.MultiplyPoint3x4(new Vector3(+baseWidth, -baseWidth, bottomLength));
            Vector3 b3 = transformMat.MultiplyPoint3x4(new Vector3(-baseWidth, -baseWidth, bottomLength));
            Vector3 b4 = transformMat.MultiplyPoint3x4(new Vector3(-baseWidth, +baseWidth, bottomLength));

            // connect the base to the top
            /*
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
            Gizmos.DrawLine(b4, bottom);*/

            Gizmos.DrawLine(end, start);
        }

        /// <summary>
        /// This is a helper function that draws bone gizmos for every bone in the hierarchy
        /// </summary>
        /// <param name="pose"></param>
        public static void DrawRigPoseGizmo(P3D_RigPose pose)
        {
            for (int i = 0; i < pose.m_rig.m_jointCount; i++)
            {
                int parentIndex = pose.m_rig.m_joints[i].m_parentIndex;

                // If this bone is the root, the parent index will be -1
                if (parentIndex == -1) continue;

                // MultiplyPoint3x4() is an optimized function that doesn't consider the last row
                // Since we are dealing with affine transformations only, we can assume the last row is always constant
                // [0 0 0 1]
                // We can also use it to quickly extract the translation component from the matrix!

                Vector3 start = pose.m_worldSpace[parentIndex].MultiplyPoint3x4(Vector3.zero);
                Vector3 end = pose.m_worldSpace[i].MultiplyPoint3x4(Vector3.zero);
                DrawBoneGizmo(start, end, pose, i);
            }
        }
        #endregion

        #region Debug Helpers
        /// <summary>
        /// This function is responsible for recursively creating a rig from a Unity Transform hierarchy
        /// </summary>
        public static void DebugInitRigFromHierarchy(Transform hierarchyParent, P3D_Rig rig)
        {
            int currentIndex = 0;

            DebugInitRigFromHierarchyRecursive(hierarchyParent.GetChild(0), ref currentIndex, -1, rig);
        }

        /// <summary>
        /// Helper function that manages the recursion of hierarchy creation
        /// </summary>
        private static void DebugInitRigFromHierarchyRecursive(Transform current, ref int currentIndex, int parentIndex, P3D_Rig rig)
        {
            // keep track of the current index to send to children to use as the parentIndex
            int self = currentIndex;
            rig.m_joints[self] = DebugInitJointFromTransform(current, parentIndex, currentIndex);

            // keeps track of where we are in the array
            currentIndex++;

            // Recursive step to continue filling out the rig
            for (int i = 0; i < current.childCount; i++)
            {
                Transform child = current.GetChild(i);
                DebugInitRigFromHierarchyRecursive(child, ref currentIndex, self, rig);
            }
        }

        /// <summary>
        /// Helper function that creates a P3D_Joint based on a passed GameObject
        /// </summary>
        public static P3D_Joint DebugInitJointFromTransform(Transform transform, int parentIndex, int selfIndex)
        {
            P3D_Joint joint = new P3D_Joint();

            joint.m_name = transform.name;
            joint.m_jointIndex = selfIndex;
            joint.m_parentIndex = parentIndex;
            joint.m_localPosition = transform.localPosition;
            joint.m_localRotation = transform.localRotation;
            joint.m_localScale = transform.transform.localScale;

            Debug.Log($"Joint name: {joint.m_name}, transform name: {transform.name}, LOCAL rotation of transform: {transform.localEulerAngles}, WORLD rotation of transform: {transform.eulerAngles}");

            return joint;
        }
        #endregion
    }
}