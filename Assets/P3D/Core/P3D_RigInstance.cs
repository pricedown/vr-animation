using UnityEngine;

// Written by Seth Riddensdale
namespace pricenerds3D
{
    /// <summary>
    /// This class will be attached to the prefab that we auto-generate upon import
    /// It should ensure that the rig always moves relative to the TRS of the Unity hierarchy
    /// This does not manage animations, think of it more as a data container
    /// </summary>
    public class P3D_RigInstance : MonoBehaviour
    {
        public P3D_Rig rig;
        public P3D_RigPose deltaPose;

        [Header("Gizmos")]
        [SerializeField]
        private bool _gizmosEnabled = true;
        [SerializeField]
        private Color _boneColor = Color.blue;

        private void Awake()
        {
            // the rig is already created upon import. we'll create the deltaPose when the game starts running
            deltaPose = new P3D_RigPose(rig);
        }

        private void Update()
        {
            // we need to also take into account the position of the game object and add that to the hips global pose
            deltaPose.m_localPose[0].m_jointTranslation = transform.position;
            deltaPose.m_localPose[0].m_jointRotation = transform.rotation;

            // this should be the last step
            deltaPose.SolveFK();
        }

        public void OnDrawGizmos()
        {
            if (!_gizmosEnabled) return;
            if (rig == null)
            {
                Debug.LogWarning("Failed to draw rig gizmos! Rig is null.");
                return;
            }

            Gizmos.color = _boneColor;

            // we'll draw the delta pose in play mode
            if (Application.isPlaying) P3D_RigHelpers.DrawRigPoseGizmo(deltaPose);
            // we'll draw the base pose in editor
            else P3D_RigHelpers.DrawRigPoseGizmo(rig.m_basePose); // this draws at 0 0 0 rn which is incorrect
        }
    }
}
