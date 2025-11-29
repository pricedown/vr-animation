using UnityEngine;

namespace pricenerds3D
{
    public class P3D_KeyframeAnimationController
    {
        public void ClipControllerUpdate(P3D_ClipController clipController, float deltaTime)
        {
            if (clipController == null)
            {
                Debug.LogWarning("Failed to update ClipController!");
                return;
            }

            float overstep = 0.0f;

            // handle timestep
            deltaTime *= clipController.playbackTimeSeconds;
            clipController.clipTimeSeconds += deltaTime;
            clipController.keyframeTimeSeconds += deltaTime;

            /*
            while ((overstep = clipController.keyframeTimeSeconds - clipController.keyframe.durationSeconds) >= 0.0f)
            {

            }*/
        }
    }

    public class P3D_ClipController
    {
        public string clipName;

        // index of clip in pool and keyframe in clip
        public int clipIndex;
        public int keyframeIndex;

        // cliptime, keyframe time, and speed in seconds
        public float clipTimeSeconds;
        public float keyframeTimeSeconds;
        public float playbackTimeSeconds;

        // clip and keyframe interpolation parameters
    }
    public enum EClipTransitionFlag
    {
        Stop,
        Play,
        Reverse,
        Skip,
        Overstep
    }
}