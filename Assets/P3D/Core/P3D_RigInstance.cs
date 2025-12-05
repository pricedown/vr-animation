using System.Collections;
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

        [Header("Debug")]
        [SerializeField]
        private P3D_ClipData _debugClipData;

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

        private void Start()
        {
            for(int i = 0; i < _debugClipData.clip.keyframes.Length; i++)
            {
                if(_debugClipData.clip.samples[_debugClipData.clip.keyframes[i].sampleIndex0] != null)
                    Debug.Log("0 = " + _debugClipData.clip.samples[_debugClipData.clip.keyframes[i].sampleIndex0].jointSamples.Length);
                if(_debugClipData.clip.samples[_debugClipData.clip.keyframes[i].sampleIndex1] != null)
                    Debug.Log("1 = " + _debugClipData.clip.samples[_debugClipData.clip.keyframes[i].sampleIndex1].jointSamples.Length);

            }

            StartCoroutine(ITestClip());
        }

        private IEnumerator ITestClip()
        {
            P3D_Sample current = _debugClipData.clip.keyframes[0].GetSample0(_debugClipData.clip);
            for (int i = 0; i < _debugClipData.clip.keyframes.Length - 1; i++)
            {
                P3D_Keyframe currentKeyframe = _debugClipData.clip.keyframes[i];
                P3D_Sample sample = currentKeyframe.GetSample1(_debugClipData.clip);

                float elapsed = 0.0f;
                float duration = 0.5f;

                while (elapsed < duration)
                {
                    deltaPose.ApplyAnimationPose(P3D_JointSample.Lerp(current.jointSamples, sample.jointSamples, elapsed / duration), deltaPose.m_rig.m_basePose);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                current = sample;
            }

            yield return null;

            StartCoroutine(ITestClip());
        }

        private void LateUpdate()
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
