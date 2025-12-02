namespace pricenerds3D
{
    public class P3D_KeyframeAnimationController
    {
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

        public P3D_Sample GetCurrentSample0()
        {
            return clip?.samples[keyframe.sampleIndex0];
        }

        public P3D_Sample GetCurrentSample1()
        {
            return clip?.samples[keyframe.sampleIndex1];
        }
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