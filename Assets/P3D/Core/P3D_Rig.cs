using System;
using UnityEngine;

// Written by Seth Riddensdale
namespace pricenerds3D
{
    // Structure based on Game Engine Architecture (Jason Gregory)

    /// <summary>
    ///     P3D_Joint(s) contain all local space orientation data relative to the parent
    ///     (P3D_Joint is our replacement for a3_HierarchyNodes)
    /// </summary>
    [Serializable]
    public class P3D_Joint
    {
        public string m_name; // readable human name
        public int m_parentIndex; // index of the parent in the rig (-1 if root)
        public int m_jointIndex; // index of self in the rig
        public Vector3 m_localPosition;
        public Quaternion m_localRotation;
        public Vector3 m_localScale;
        public Matrix4x4 m_bindMatrix; // world space matrix of bind pose
        public Matrix4x4 m_bindMatrixInverse; // inverse of m_bindMatrix
    }

    /// <summary>
    ///     The rig class is a data structure that holds all of our joints.
    ///     This would essentially act as our "base pose" that describes the initial state of the rig before applying any
    ///     affine transformations to it
    /// </summary>
    [Serializable]
    public class P3D_Rig
    {
        public uint m_jointCount; // number of joints
        public P3D_Joint[] m_joints; // array of joints
        public P3D_RigPose m_basePose;

        public P3D_Rig(uint jointCount)
        {
            m_jointCount = jointCount;
            m_joints = new P3D_Joint[jointCount];

            // we need to ensure all joints are initialized
            for (var i = 0; i < m_jointCount; i++) m_joints[i] = new P3D_Joint();
        }

        public void InitializeBasePose()
        {
            // create base pose and set bind matrices
            m_basePose = new P3D_RigPose(this);
            ComputeBindMatrices();
        }

        /// <summary>
        ///     This should be done only once on initialization. put this in rig instance when ready
        /// </summary>
        public void ComputeBindMatrices()
        {
            for (var i = 0; i < m_jointCount; i++)
            {
                m_joints[i].m_bindMatrix = m_basePose.m_worldSpace[i];
                m_joints[i].m_bindMatrixInverse = m_basePose.m_worldSpace[i].inverse;
            }
        }

        /// <summary>
        ///     Helper function that retrieves a joint with the given name
        /// </summary>
        public P3D_Joint GetJointFromName(string name)
        {
            for (var i = 0; i < m_jointCount; i++)
                if (name == m_joints[i].m_name)
                    return m_joints[i];

            return null;
        }
    }

    /// <summary>
    ///     The P3D_RigPose contains a reference to the rig, along with all the local / world space poses of each joint
    ///     Contains an array of P3D_JointPoses that describe the local space transformations
    ///     Contains an array of matrices that describe how the local space transformations are described in world space
    /// </summary>
    [Serializable]
    public class P3D_RigPose
    {
        public P3D_Rig m_rig; // do not modify the joint positions here. we read from this
        public P3D_JointPose[] m_localPose; // these we modify
        public Matrix4x4[] m_worldSpace; // this is calculated in our FK algorithm

        public Matrix4x4[] m_worldSpaceInverse;
        public Matrix4x4[] m_localSpaceInverse;

        // Default constructor
        // (NOTE: you do need to ensure that m_localPose and m_worldSpace pose are initialized if you use this)
        public P3D_RigPose()
        {
        }

        // This constructor will take in a rig and automatically calculate local and world space pose data
        public P3D_RigPose(P3D_Rig rig)
        {
            m_rig = rig;
            m_worldSpaceInverse = new Matrix4x4[m_rig.m_jointCount];
            m_localSpaceInverse = new Matrix4x4[m_rig.m_jointCount];

            m_localPose = new P3D_JointPose[m_rig.m_jointCount];
            m_worldSpace = new Matrix4x4[m_rig.m_jointCount];

            // initialize with local pose values
            for (var i = 0; i < m_rig.m_jointCount; i++)
            {
                m_localPose[i].m_jointTranslation = rig.m_joints[i].m_localPosition;
                m_localPose[i].m_jointRotation = rig.m_joints[i].m_localRotation;
                m_localPose[i].m_jointScale = rig.m_joints[i].m_localScale;
            }

            // run FK algorithm
            SolveFK();
        }

        /// <summary>
        /// Applies animation deltas to base pose by adding translation, multiplying rotation, and scaling
        /// </summary>
        /// <param name="animPose">The delta pose to be concatenated onto base pose</param>
        /// <param name="basePose">The base pose</param>
        public void ApplyAnimationPose(P3D_JointSample[] animPose, P3D_RigPose basePose)
        {
            var count = Mathf.Min(animPose.Length, (int)m_rig.m_jointCount);
            for (var i = 0; i < count; i++)
            {
                m_localPose[i].m_jointTranslation =
                    basePose.m_localPose[i].m_jointTranslation + animPose[i].translation;
                m_localPose[i].m_jointRotation = basePose.m_localPose[i].m_jointRotation * animPose[i].rotation;
                m_localPose[i].m_jointScale = Vector3.Scale(basePose.m_localPose[i].m_jointScale, animPose[i].scale);
            }
        }


        /// <summary>
        ///     Computes local matrix to use for calculating world space matrices
        /// </summary>
        public void SolveFK()
        {
            // solve forward kinematics
            for (var i = 0; i < m_rig.m_jointCount; i++)
            {
                var joint = m_rig.m_joints[i];

                var localPoseMatrix = Matrix4x4.TRS(
                    m_localPose[i].m_jointTranslation,
                    m_localPose[i].m_jointRotation,
                    m_localPose[i].m_jointScale);

                if (joint.m_parentIndex == -1) m_worldSpace[i] = localPoseMatrix;
                else m_worldSpace[i] = m_worldSpace[joint.m_parentIndex] * localPoseMatrix;

                //Quaternion worldRot = m_worldSpace[i].rotation;
                //m_localPose[i].m_basis = Matrix4x4.Rotate(worldRot);

                // Bases are currently all set relative to world space
                m_localPose[i].m_basis.SetColumn(0, Vector3.right);
                m_localPose[i].m_basis.SetColumn(1, Vector3.up);
                m_localPose[i].m_basis.SetColumn(2, Vector3.forward);
                m_localPose[i].m_basis.SetColumn(3, new Vector4(0, 0, 0, 1));
            }
        }

        /// <summary>
        /// Converts world space matrices back to local space pose data (TRS)
        /// </summary>
        public void SolveIK()
        {
            for (var i = 0; i < m_rig.m_jointCount; i++)
            {
                var joint = m_rig.m_joints[i];
                if (joint.m_parentIndex == -1)
                {
                    RestorePoseFromMatrix(i, m_worldSpace[i]);
                }
                else
                {
                    var local = m_worldSpaceInverse[joint.m_parentIndex] * m_worldSpace[i];
                    RestorePoseFromMatrix(i, local);
                }
            }
        }

        /// <summary>
        /// Extracts TRS components from a transformation matrix
        /// </summary>
        /// <param name="index">localPose index</param>
        /// <param name="mat">matrix</param>
        private void RestorePoseFromMatrix(int index, Matrix4x4 mat)
        {
            m_localPose[index].m_jointTranslation = mat.GetColumn(3);

            var scale = new Vector3(
                mat.GetColumn(0).magnitude,
                mat.GetColumn(1).magnitude,
                mat.GetColumn(2).magnitude);
            m_localPose[index].m_jointScale = scale;

            if (scale.x > 0.0001f && scale.y > 0.0001f && scale.z > 0.0001f)
            {
                var rotMat = Matrix4x4.identity;
                rotMat.SetColumn(0, mat.GetColumn(0) / scale.x);
                rotMat.SetColumn(1, mat.GetColumn(1) / scale.y);
                rotMat.SetColumn(2, mat.GetColumn(2) / scale.z);
                m_localPose[index].m_jointRotation = rotMat.rotation;
            }
        }

        /// <summary>
        /// Copies local pose transformations from another pose
        /// </summary>
        /// <param name="otherPose"></param>
        public void CopyLocalPose(P3D_RigPose otherPose)
        {
            for (var i = 0; i < m_rig.m_jointCount; i++)
            {
                m_localPose[i].m_jointTranslation = otherPose.m_localPose[i].m_jointTranslation;
                m_localPose[i].m_jointRotation = otherPose.m_localPose[i].m_jointRotation;
                m_localPose[i].m_jointScale = otherPose.m_localPose[i].m_jointScale;
            }
        }

        /// <summary>
        /// Caches inverse world space matrices for all joints
        /// </summary>
        public void ComputeWorldInverse()
        {
            for (var i = 0; i < m_rig.m_jointCount; i++)
                m_worldSpaceInverse[i] = m_worldSpace[i].inverse;
        }

        /// <summary>
        /// Caches inverse local space matrices for all joints
        /// </summary>
        public void ComputeLocalInverse()
        {
            for (var i = 0; i < m_rig.m_jointCount; i++)
            {
                var local = Matrix4x4.TRS(
                    m_localPose[i].m_jointTranslation,
                    m_localPose[i].m_jointRotation,
                    m_localPose[i].m_jointScale);
                m_localSpaceInverse[i] = local.inverse;
            }
        }
    }

    /// <summary>
    ///     P3D_JointPose describes an affine, local transformation
    ///     - (This is essentially our a3_SpatialPose)
    /// </summary>
    [Serializable]
    public struct P3D_JointPose // a joint pose is an affine transformation
    {
        public Quaternion m_jointRotation;
        public Vector3 m_jointTranslation;
        public Vector3 m_jointScale;
        public Matrix4x4 m_basis;

        public Vector3 m_jointRight => m_basis.GetColumn(0);
        public Vector3 m_jointUp => m_basis.GetColumn(1);
        public Vector3 m_jointForward => m_basis.GetColumn(2);
    }
}