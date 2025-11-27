using System;
using UnityEngine;

// Written by Seth Riddensdale
namespace pricenerds3D
{
    public class P3D_SkeletonDebug : MonoBehaviour
    {
        [SerializeField]
        private Transform _sampleHierarchy;

        [Header("Gizmos")]
        [SerializeField]
        private bool _gizmosEnabled = true;
        [SerializeField]
        private Color _boneColor = Color.blue;

        public P3D_Skeleton skeleton;
        public P3D_SkeletonPose basePose;

        private void OnEnable()
        {
            // this skeleton should be created at import instead of now. just putting it here for now   
            InitializeSkeleton();
            basePose = new P3D_SkeletonPose(skeleton);
        }

        // This is essentially a backwards way of creating a skeleton. We already have the skeleton using GameObjects for testing purposes. We have to replace this later with a custom importer
        public void InitializeSkeleton()
        {
            skeleton = new P3D_Skeleton((UInt32)_sampleHierarchy.CountAllChildren());
            P3D_SkeletonHelpers.DebugInitSkeletonFromHierarchy(_sampleHierarchy, skeleton);
        }

        // this will have to be in a custom editor that reads from some kind of data file
        public void OnDrawGizmos()
        {
            if (!_gizmosEnabled) return;
            if (skeleton == null)
            {
                Debug.LogWarning("Failed to draw skeleton gizmos! Skeleton is null.");
                return;
            }

            Gizmos.color = _boneColor;
            P3D_SkeletonHelpers.DrawSkeletonPoseGizmo(basePose);
        }
    }
}