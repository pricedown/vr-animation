using UnityEngine;

namespace pricenerds3D
{
    /// <summary>
    /// This will be responsible for blending
    /// </summary>
    public class P3D_KeyframeAnimationController : MonoBehaviour 
    {
        [SerializeField] private P3D_ClipData _debugClip;

        public P3D_ClipController clipController;

        private void Awake()
        {
            clipController = new P3D_ClipController(_debugClip.clip);
        }

        private void Update()
        {
            // TO DO: Fix the infinite loop here
            //clipController.ClipControllerUpdate(Time.deltaTime);
        }
    }

    public class P3D_ClipController
    {
        public P3D_Clip clip;
        public string clipName;
        public float clipParam;

        // timing
        public float clipTimeSeconds;
        public P3D_Keyframe keyframe;
        public int keyframeIndex;

        // interpolation parameters
        public float keyframeParam;
        public float keyframeTimeSeconds;
        public float playbackSpeed = 1f;

        public P3D_ClipController(P3D_Clip clip)
        {
            this.clip = clip;
            keyframeIndex = 0;
            clipTimeSeconds = 0f;
            keyframeTimeSeconds = 0f;
            keyframeParam = 0f;
            clipParam = 0f;
            if (clip != null && clip.keyframes.Length > 0)
                keyframe = clip.keyframes[0];
        }

        /// <summary>
        /// Plays a clip controller forward by deltaTime, resolving its keyframe and state
        /// </summary>
        /// <param name="clipController">The clip controller which is on a timeline</param>
        /// <param name="deltaTime">The amount of seconds passed since last update</param>
        public void ClipControllerUpdate(float deltaTime)
        {
            if (clip == null) return;
            if (clip.keyframes.Length == 0) return;

            deltaTime *= playbackSpeed;
            clipTimeSeconds += deltaTime;
            keyframeTimeSeconds += deltaTime;

            float overstep;

            while ((overstep = keyframeTimeSeconds - keyframe.duration) >= 0f)
            {
                if (keyframeIndex >= clip.keyframes.Length - 1)
                {
                    keyframeIndex = 0;
                    keyframe = clip.keyframes[0];
                    keyframeTimeSeconds = overstep;
                    clipTimeSeconds = overstep;
                }
                else
                {
                    keyframeIndex++;
                    keyframe = clip.keyframes[keyframeIndex];
                    keyframeTimeSeconds = overstep;
                }
            }

            while ((overstep = keyframeTimeSeconds) < 0f)
            {
                if (keyframeIndex <= 0)
                {
                    keyframeIndex = clip.keyframes.Length - 1;
                    keyframe = clip.keyframes[keyframeIndex];
                    keyframeTimeSeconds = keyframe.duration + overstep;
                    clipTimeSeconds = clip.durationSeconds + overstep;
                }
                else
                {
                    keyframeIndex--;
                    keyframe = clip.keyframes[keyframeIndex];
                    keyframeTimeSeconds = keyframe.duration + overstep;
                }
            }

            keyframeParam = keyframeTimeSeconds * keyframe.durationInverse;
            clipParam = clipTimeSeconds * clip.durationInverse;
        }

        /// <summary>
        /// Retrieve beginning sample
        /// </summary>
        /// <returns>First sample of keyframe</returns>
        public P3D_Sample GetCurrentSample0()
        {
            return clip?.samples[keyframe.sampleIndex0];
        }

        /// <summary>
        /// Retrieve ending sample
        /// </summary>
        /// <returns>Last sample of keyframe</returns>
        public P3D_Sample GetCurrentSample1()
        {
            return clip?.samples[keyframe.sampleIndex1];
        }

        /// <summary>
        /// Retrieves a joint's current state as interpolated between samples within keyframe
        /// </summary>
        /// <param name="jointIndex">Index of the joint</param>
        /// <returns>Interpolated sample of joint</returns>
        public P3D_JointSample GetInterpolatedJoint(int jointIndex)
        {
            if (clip == null) return P3D_JointSample.Identity;
            var s0 = GetCurrentSample0();
            var s1 = GetCurrentSample1();
            if (s0 == null || s1 == null) return P3D_JointSample.Identity;
            return P3D_JointSample.Lerp(s0.GetJointPose(jointIndex), s1.GetJointPose(jointIndex), keyframeParam);
        }

        /// <summary>
        /// Retrieves current pose state as interpolated between samples within the keyframe
        /// </summary>
        /// <param name="outPose">Resulting interpolated pose</param>
        public void GetInterpolatedPose(P3D_JointSample[] outPose)
        {
            if (clip == null || outPose == null) return;
            var s0 = GetCurrentSample0();
            var s1 = GetCurrentSample1();
            if (s0 == null || s1 == null) return;

            var count = Mathf.Min(outPose.Length, s0.jointCount);
            for (var i = 0; i < count; i++)
                outPose[i] = P3D_JointSample.Lerp(s0.GetJointPose(i), s1.GetJointPose(i), keyframeParam);
        }
    }

    /// <summary>
    /// Signals how a clip should end
    /// </summary>
    public enum EClipTransitionFlag
    {
        Stop,
        Play,
        Reverse,
        Skip,
        Overstep
    }
}