using UnityEngine;

namespace pricenerds3D
{
    public static class P3D_SkeletonHelpers
    {
        public static void DrawBone(Vector3 p0, Vector3 p1)
        {
            Vector3 dir = p1 - p0;
            float length = dir.magnitude;
            if (length < 0.0001f) return;

            dir /= length;

            // orientation basis
            Vector3 zOrtho = dir;
            Vector3 xOrtho = Vector3.Cross(zOrtho, Vector3.right.normalized);
            Vector3 yOrtho = Vector3.Cross(zOrtho, xOrtho);

            // build transform matrix
            Matrix4x4 transformMat = new Matrix4x4();
            transformMat.SetColumn(0, new Vector4(xOrtho.x, xOrtho.y, xOrtho.z, 0));
            transformMat.SetColumn(1, new Vector4(yOrtho.x, yOrtho.y, yOrtho.z, 0));
            transformMat.SetColumn(2, new Vector4(zOrtho.x, zOrtho.y, zOrtho.z, 0));
            transformMat.SetColumn(3, new Vector4(p0.x, p0.y, p0.z, 1));

            float w = length * 0.1f; // base width

            float topLength = length * 0.9f;
            float bottomLength = length * 0.1f;

            Vector3 top = transformMat.MultiplyPoint3x4(new Vector3(0, 0, topLength + bottomLength));
            Vector3 bottom = transformMat.MultiplyPoint3x4(new Vector3(0, 0, 0.0f));
            Vector3 b1 = transformMat.MultiplyPoint3x4(new Vector3(+w, +w, bottomLength));
            Vector3 b2 = transformMat.MultiplyPoint3x4(new Vector3(+w, -w, bottomLength));
            Vector3 b3 = transformMat.MultiplyPoint3x4(new Vector3(-w, -w, bottomLength));
            Vector3 b4 = transformMat.MultiplyPoint3x4(new Vector3(-w, +w, bottomLength));

            Gizmos.DrawLine(b1, top);
            Gizmos.DrawLine(b2, top);
            Gizmos.DrawLine(b3, top);
            Gizmos.DrawLine(b4, top);

            Gizmos.DrawLine(b1, b2);
            Gizmos.DrawLine(b2, b3);
            Gizmos.DrawLine(b3, b4);
            Gizmos.DrawLine(b4, b1);

            Gizmos.DrawLine(b1, bottom);
            Gizmos.DrawLine(b2, bottom);
            Gizmos.DrawLine(b3, bottom);
            Gizmos.DrawLine(b4, bottom);
        }
    }
}