using System;
using UnityEngine;

namespace pricenerds3D
{
    // structure based on Game Engine Architecture (Jason Gregory)
    public struct P3D_Joint
    {
        public string m_name;                   // readable human name
        public sbyte m_parentIndex;              // index of the parent in the skeleton (-1 if root)
        public Vector3 m_localPosition;
        public Quaternion m_localRotation;
        public Vector3 m_localScale;
        public Matrix4x4 m_bindMatrix;          // world space matrix of bind pose
        public Matrix4x4 m_bindMatrixInverse;   // inverse of m_bindMatrix
    }

    public class P3D_Skeleton
    {
        public UInt32 m_jointCount;             // number of joints
        public P3D_Joint[] m_joints;            // array of joints

        public P3D_Skeleton(UInt32 jointCount)
        {
            this.m_jointCount = jointCount;

            m_joints = new P3D_Joint[jointCount];
        }
    }

    public class P3D_SkeletonPose
    {
        P3D_Skeleton m_skeleton;
        P3D_JointPose[] m_localPose;
    }

    public struct P3D_JointPose // a joint pose is an affine transformation
    {
        public Quaternion m_jointRotation;
        public Vector3 m_jointTranslation;
        public Vector3 m_jointScale;
    }
}
