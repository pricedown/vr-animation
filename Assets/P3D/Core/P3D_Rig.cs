using System;
using UnityEngine;

// Written by Seth Riddensdale
namespace pricenerds3D
{
    // Structure based on Game Engine Architecture (Jason Gregory)

    /// <summary>
    /// P3D_Joint(s) contain all local space orientation data relative to the parent
    /// (P3D_Joint is our replacement for a3_HierarchyNodes)
    /// </summary>
    [Serializable]
    public class P3D_Joint
    {
        public string m_name;                   // readable human name
        public int m_parentIndex;             // index of the parent in the rig (-1 if root)
        public int m_jointIndex;              // index of self in the rig
        public Vector3 m_localPosition;
        public Quaternion m_localRotation;
        public Vector3 m_localScale;
        public Matrix4x4 m_bindMatrix;          // world space matrix of bind pose
        public Matrix4x4 m_bindMatrixInverse;   // inverse of m_bindMatrix
    }

    /// <summary>
    /// The rig class is a data structure that holds all of our joints.
    /// This would essentially act as our "base pose" that describes the initial state of the rig before applying any affine transformations to it
    /// </summary>
    [Serializable]
    public class P3D_Rig
    {
        public uint m_jointCount;               // number of joints
        public P3D_Joint[] m_joints;            // array of joints

        public P3D_Rig(uint jointCount)
        {
            m_jointCount = jointCount;
            m_joints = new P3D_Joint[jointCount];
        }

        public P3D_Joint GetJointFromName(string name)
        {
            for(int i = 0; i < m_jointCount; i++)
            {
                if(name == m_joints[i].m_name)
                {
                    return m_joints[i];
                }
            }

            return null;
        }
    }

    /// <summary>
    /// The P3D_RigPose contains a reference to the rig, along with all the local / world space poses of each joint
    /// Contains an array of P3D_JointPoses that describe the local space transformations
    /// Contains an array of matrices that describe how the local space transformations are described in world space
    /// </summary>
    [Serializable]
    public class P3D_RigPose
    {
        public P3D_Rig m_rig;
        public P3D_JointPose[] m_localPose;
        public Matrix4x4[] m_worldSpace;

        // Default constructor
        // (NOTE: you do need to ensure that m_localPose and m_worldSpace pose are initialized if you use this)
        public P3D_RigPose() { }

        // This constructor will take in a rig and automatically calculate local and world space pose data
        public P3D_RigPose(P3D_Rig rig)
        {
            m_rig = rig;
            m_localPose = new P3D_JointPose[m_rig.m_jointCount];
            m_worldSpace = new Matrix4x4[m_rig.m_jointCount];

            // initialize with local pose values
            for (int i = 0; i < m_rig.m_jointCount; i++)
            {
                m_localPose[i].m_jointTranslation = rig.m_joints[i].m_localPosition;
                m_localPose[i].m_jointRotation = rig.m_joints[i].m_localRotation;
                m_localPose[i].m_jointScale = rig.m_joints[i].m_localScale;
            }

            // run FK algorithm
            SolveFK();
        }

        /// <summary>
        /// Computes local matrix to use for calculating world space matrices
        /// </summary>
        public void SolveFK()
        {
            for(int i = 0; i < m_rig.m_jointCount; i++)
            {
                P3D_Joint joint = m_rig.m_joints[i];

                Matrix4x4 localPoseMatrix = Matrix4x4.TRS(
                    m_localPose[i].m_jointTranslation,
                    m_localPose[i].m_jointRotation,
                    m_localPose[i].m_jointScale);

                if (joint.m_parentIndex == -1) m_worldSpace[i] = localPoseMatrix;
                else m_worldSpace[i] = m_worldSpace[joint.m_parentIndex] * localPoseMatrix;
            }
        }
    }

    /// <summary>
    /// P3D_JointPose describes an affine, local transformation
    /// - (This is essentially our a3_SpatialPose)
    /// </summary>
    [Serializable]
    public struct P3D_JointPose // a joint pose is an affine transformation
    {
        public Quaternion m_jointRotation;
        public Vector3 m_jointTranslation;
        public Vector3 m_jointScale;

        public P3D_JointPose(Vector3 translation, Quaternion rotation, Vector3 scale)
        {
            m_jointTranslation = translation;
            m_jointRotation = rotation;
            m_jointScale = scale;
        }
    }
}
