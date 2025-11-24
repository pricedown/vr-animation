using System;
using UnityEngine;

namespace pricenerds3D
{
    // structure based on Game Engine Architecture (Jason Gregory)
    [System.Serializable]
    public struct P3D_Joint
    {
        public string m_name;                   // readable human name
        public sbyte m_parentIndex;             // index of the parent in the skeleton (-1 if root)
        public Vector3 m_localPosition;
        public Quaternion m_localRotation;
        public Vector3 m_localScale;
        public Matrix4x4 m_bindMatrix;          // world space matrix of bind pose
        public Matrix4x4 m_bindMatrixInverse;   // inverse of m_bindMatrix
    }

    [System.Serializable]
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
        public P3D_Skeleton m_skeleton;
        public P3D_JointPose[] m_localPose;
        public Matrix4x4[] m_worldSpace;

        // default constructor (NOTE: you need to ensure that m_localPose and m_worldSpace pose are initialized if you use this)
        public P3D_SkeletonPose() { }

        // this constructor will take in a skeleton and automatically calculate local and world space pose data
        public P3D_SkeletonPose(P3D_Skeleton skeleton)
        {
            m_skeleton = skeleton;
            m_localPose = new P3D_JointPose[m_skeleton.m_jointCount];
            m_worldSpace = new Matrix4x4[m_skeleton.m_jointCount];

            for (int i = 0; i < m_skeleton.m_jointCount; i++)
            {
                // 1. initialize local pose values based on input skeleton
                m_localPose[i].m_jointTranslation = skeleton.m_joints[i].m_localPosition;
                m_localPose[i].m_jointRotation = skeleton.m_joints[i].m_localRotation;
                m_localPose[i].m_jointScale = skeleton.m_joints[i].m_localScale;

                // 2. calculate world space pose
                P3D_Joint joint = m_skeleton.m_joints[i];

                Matrix4x4 localPoseMatrix = Matrix4x4.TRS(
                    m_localPose[i].m_jointTranslation,
                    m_localPose[i].m_jointRotation,
                    m_localPose[i].m_jointScale);

                // if we don't have a parent, we are the root
                if (joint.m_parentIndex == -1) m_worldSpace[i] = localPoseMatrix;
                // otherwise, compute world space of local bone 
                else m_worldSpace[i] = m_worldSpace[joint.m_parentIndex] * localPoseMatrix;
            }
        }
    }

    public struct P3D_JointPose // a joint pose is an affine transformation
    {
        public Quaternion m_jointRotation;
        public Vector3 m_jointTranslation;
        public Vector3 m_jointScale;
    }
}
