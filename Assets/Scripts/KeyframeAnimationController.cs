using UnityEngine;

public class KeyframeAnimationController
{
    public void ClipControllerUpdate(ClipController clipController, float deltaTime)
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

public class ClipController
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

public class ClipPool
{
    Clip[] clip;
}

public class Clip
{
    Keyframe[] keyframe;

    int index;
    int keyframeIndexFirst;
    int keyframeIndexLast;
    int keyframeDirection; // either 1 or -1
    float durationSeconds;
}

public class Keyframe
{
    Sample[] samples;

    int index;
    uint sampleIndex0;
    uint sampleIndex1;
    float duration;
}

public class Sample
{
    int index;
    float timeSeconds;
}
