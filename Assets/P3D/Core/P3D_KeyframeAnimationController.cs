using UnityEngine;

namespace pricenerds3D
{
    public class P3D_KeyframeAnimationController
    {
        /// <summary>
        /// Plays a clip controller forward by deltaTime, resolving its keyframe and state
        /// </summary>
        /// <param name="clipController">The clip controller which is on a timeline</param>
        /// <param name="deltaTime">The amount of seconds passed since last update</param>
        public void ClipControllerUpdate(P3D_ClipController clipController, float deltaTime)
        {
            if (clipController == null || clipController.clip == null) return;
            var clip = clipController.clip;
            if (clip.keyframes.Length == 0) return;

            deltaTime *= clipController.playbackSpeed;
            clipController.clipTimeSeconds += deltaTime;
            clipController.keyframeTimeSeconds += deltaTime;

            float overstep;

            while ((overstep = clipController.keyframeTimeSeconds - clipController.keyframe.duration) >= 0f)
                if (clipController.keyframeIndex >= clip.keyframes.Length - 1)
                {
                    clipController.keyframeIndex = 0;
                    clipController.keyframe = clip.keyframes[0];
                    clipController.keyframeTimeSeconds = overstep;
                    clipController.clipTimeSeconds = overstep;
                }
                else
                {
                    clipController.keyframeIndex++;
                    clipController.keyframe = clip.keyframes[clipController.keyframeIndex];
                    clipController.keyframeTimeSeconds = overstep;
                }

            while ((overstep = clipController.keyframeTimeSeconds) < 0f)
                if (clipController.keyframeIndex <= 0)
                {
                    clipController.keyframeIndex = clip.keyframes.Length - 1;
                    clipController.keyframe = clip.keyframes[clipController.keyframeIndex];
                    clipController.keyframeTimeSeconds = clipController.keyframe.duration + overstep;
                    clipController.clipTimeSeconds = clip.durationSeconds + overstep;
                }
                else
                {
                    clipController.keyframeIndex--;
                    clipController.keyframe = clip.keyframes[clipController.keyframeIndex];
                    clipController.keyframeTimeSeconds = clipController.keyframe.duration + overstep;
                }

            clipController.keyframeParam = clipController.keyframeTimeSeconds * clipController.keyframe.durationInverse;
            clipController.clipParam = clipController.clipTimeSeconds * clip.durationInverse;
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

        public void Initialize(P3D_Clip _clip)
        {
            clip = _clip;
            keyframeIndex = 0;
            clipTimeSeconds = 0f;
            keyframeTimeSeconds = 0f;
            keyframeParam = 0f;
            clipParam = 0f;
            if (clip != null && clip.keyframes.Length > 0)
                keyframe = clip.keyframes[0];
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