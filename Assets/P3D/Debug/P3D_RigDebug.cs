using System;
using UnityEngine;

// Written by Seth Riddensdale
namespace pricenerds3D
{
    public class P3D_RigDebug : MonoBehaviour
    {
        [SerializeField]
        private Transform _sampleHierarchy;
        [SerializeField]
        private P3D_ClipData _testAnimation;

        [Header("Gizmos")]
        [SerializeField]
        private Transform _tracker;
        [SerializeField]
        private string _boneToTrack;
        [SerializeField]
        private bool _gizmosEnabled = true;
        [SerializeField]
        private Color _boneColor = Color.blue;

        public P3D_Rig rig;
        public P3D_RigPose deltaPose;
        private int jointIndex;
        private Quaternion initialJointRotation;

        private void OnEnable()
        {
            // this rig should be created at import instead of now. just putting it here for now   
            InitializeRig();
            rig.InitializeBasePose();
            deltaPose = new P3D_RigPose(rig);

            // DEBUG
            jointIndex = deltaPose.m_rig.GetJointFromName(_boneToTrack).m_jointIndex;
            initialJointRotation = deltaPose.m_localPose[jointIndex].m_jointRotation;
            //_tracker.rotation = deltaPose.m_localPose[jointIndex].m_jointRotation;
        }

        // This is essentially a backwards way of creating a rig. We already have the rig using GameObjects for testing purposes. We have to replace this later with a custom importer
        public void InitializeRig()
        {
            rig = new P3D_Rig((uint)_sampleHierarchy.CountAllChildren());
            P3D_RigHelpers.DebugInitRigFromHierarchy(_sampleHierarchy, rig);
        }

        private void Update()
        {
            // DEBUG

            deltaPose.m_localPose[jointIndex].m_jointRotation = _tracker.localRotation * Quaternion.Inverse(initialJointRotation);
            deltaPose.SolveFK();

            Debug.DrawRay(_tracker.position, _tracker.right * 0.05f, Color.red);
            Debug.DrawRay(_tracker.position, _tracker.up * 0.05f, Color.green);
            Debug.DrawRay(_tracker.position, _tracker.forward * 0.05f, Color.blue);
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
            P3D_RigHelpers.DrawRigPoseGizmo(deltaPose);
        }
    }
}