using System.Collections.Generic;
using UnityEngine;

// Written by Seth Riddensdale
namespace pricenerds3D
{
    /// <summary>
    ///     This is a sub-container that we use to keep track of each animation processed by the HTR file
    ///     This is not the final representation of the data, we just use this to populate with raw HTR data to be sent to a
    ///     useful structure like a scriptable object
    /// </summary>
    public class P3D_HTRAnimationDataContainer
    {
        public string animationName;
        public uint numFrames;

        // We use the dictionaries to quickly access position / rotation of each joint by simply using the name of the bone
        public Dictionary<string, Vector3>[] position;
        public Dictionary<string, Quaternion>[] rotation;
        public Dictionary<string, Vector3>[] scale;

        public P3D_HTRAnimationDataContainer(string animationName, uint numFrames)
        {
            this.numFrames = numFrames;
            this.animationName = animationName;

            // initialize dictionary arrays
            position = new Dictionary<string, Vector3>[numFrames];
            rotation = new Dictionary<string, Quaternion>[numFrames];

            // initialize each dictionary
            for (var i = 0; i < numFrames; i++)
            {
                position[i] = new Dictionary<string, Vector3>();
                rotation[i] = new Dictionary<string, Quaternion>();
            }

            scale = new Dictionary<string, Vector3>[numFrames];
            for (var i = 0; i < numFrames; i++) scale[i] = new Dictionary<string, Vector3>();
        }
    }
}