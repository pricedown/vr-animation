using UnityEngine;

namespace pricenerds3D
{
    /// <summary>
    /// This will be responsible for blending
    /// </summary>
    public class P3D_KeyframeAnimationController : MonoBehaviour 
    {
        [SerializeField] 
        private P3D_ClipData _clip;
        [SerializeField] 
        private P3D_ClipData _clipB;
        [SerializeField, Range(0, 1)] 
        private float _blendingFactor;

        [SerializeField, Min(0)] private float _playbackSpeed = 1.0f;

        public P3D_ClipController clipController;
        public P3D_ClipController clipControllerB;
        public P3D_JointSample[] blendedPose;

        private void Awake()
        {
            clipController = new P3D_ClipController(_clip.clip);
            clipControllerB = new P3D_ClipController(_clipB.clip);
            blendedPose = new P3D_JointSample[_clip.clip.jointCount];
        }

        private void Update()
        {
            //_animationController.clipController.GetInterpolatedPose()
            clipController.playbackSpeed = _playbackSpeed;
            clipController.playbackSpeed = _playbackSpeed;

            clipController.ClipControllerUpdate(Time.deltaTime);
            clipControllerB.ClipControllerUpdate(Time.deltaTime);

            P3D_JointSample[] sampleA = clipController.GetInterpolatedPose();
            P3D_JointSample[] sampleB = clipControllerB.GetInterpolatedPose();

            // blend
            for (var i = 0; i < blendedPose.Length; i++)
                blendedPose[i] = P3D_JointSample.Lerp(sampleA[i], sampleB[i], _blendingFactor);
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
        public P3D_JointSample[] GetInterpolatedPose()
        {
            if (clip == null) return new P3D_JointSample[0];
            var s0 = GetCurrentSample0();
            var s1 = GetCurrentSample1();
            if (s0 == null || s1 == null) return new P3D_JointSample[0];

            P3D_JointSample[] pose = new P3D_JointSample[(int)clip.jointCount];
            var count = Mathf.Min(pose.Length, s0.jointCount);
            for (var i = 0; i < count; i++)
                pose[i] = P3D_JointSample.Lerp(s0.GetJointPose(i), s1.GetJointPose(i), keyframeParam);

            return pose;
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