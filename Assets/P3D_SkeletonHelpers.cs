using UnityEngine;

namespace pricenerds3D
{
    public static class P3D_SkeletonHelpers
    {
        // notes for joe:
        // this is a helper function I wrote for drawing a bone from a start to end 
        // it draws a bone similar to how a program like blender / maya would
        // you call it in a recursive loop to draw all bones of a skeletal hierarchy
        public static void P3D_DrawBoneGizmo(Vector3 start, Vector3 end)
        {
            Vector3 dir = end - start;
            float length = dir.magnitude;

            // avoid division by zero errors
            if (length == 0.0f) return;
            dir /= length;

            // create an orthonormal basis
            Vector3 zOrtho = dir;
            Vector3 xOrtho = Vector3.Cross(zOrtho, Vector3.right).normalized;
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
        public static void P3D_DrawSkeletonGizmo(P3D_Skeleton skeleton) // note for later, pass in P3D_SkeletonPose, not P3D_Skeleton
        {
            for (int i = 0; i < skeleton.m_joints.Length - 1; i++)
            {
                P3D_Joint currentJoint = skeleton.m_joints[i];
                P3D_Joint nextJoint = skeleton.m_joints[i + 1];

                P3D_DrawBoneGizmo(currentJoint.m_localPosition, nextJoint.m_localPosition);
            }
        }
    }
}