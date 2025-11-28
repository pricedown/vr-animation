using System;
using UnityEngine;

// Written by Seth Riddensdale
namespace pricenerds3D
{
    public class P3D_RigDebug : MonoBehaviour
    {
        [SerializeField]
        private Transform _sampleHierarchy;

        [Header("Gizmos")]
        [SerializeField]
        private bool _gizmosEnabled = true;
        [SerializeField]
        private Color _boneColor = Color.blue;

        public P3D_Rig rig;
        public P3D_RigPose basePose;

        private void OnEnable()
        {
            // this rig should be created at import instead of now. just putting it here for now   
            InitializeRig();
            basePose = new P3D_RigPose(rig);
        }

        // This is essentially a backwards way of creating a rig. We already have the rig using GameObjects for testing purposes. We have to replace this later with a custom importer
        public void InitializeRig()
        {
            rig = new P3D_Rig((UInt32)_sampleHierarchy.CountAllChildren());
            P3D_RigHelpers.DebugInitRigFromHierarchy(_sampleHierarchy, rig);
        }

        // this will have to be in a custom editor that reads from some kind of data file
        public void OnDrawGizmos()
        {
            if (!_gizmosEnabled) return;
            if (rig == null)
            {
                Debug.LogWarning("Failed to draw rig gizmos! Rig is null.");
                return;
            }

            Gizmos.color = _boneColor;
            P3D_RigHelpers.DrawRigPoseGizmo(basePose);
        }
    }
}