using UnityEngine;
using System.Collections.Generic;

// Written by Seth Riddensdale
namespace pricenerds3D
{
    /// <summary>
    /// This is a data container that contains all important data loaded from an HTR file
    /// </summary>
    public class P3D_HTRDataContainer
    {
        public uint numSegments;    // the amount of bones in our hierarchy
        public uint totalFrames;    // the total frames of all animations in the file
        public uint frameRate;      // useful comment about frame rate

        public List<string> segmentNames;
        public Dictionary<string, string> segmentHierarchy;         // child (string) -> parent (string)
        public Dictionary<string, Vector3> basePosePosition;        // child (string) -> position (vector3)
        public Dictionary<string, Quaternion> basePoseRotation;     // child (string) -> rotation (quaternion)
        public List<P3D_HTRAnimationDataContainer> animationData;   // list of each animation processed by the htr loader

        public P3D_HTRDataContainer()
        {
            // constructor initializes all of our structures
            segmentHierarchy = new();
            basePosePosition = new();
            basePoseRotation = new();
            animationData = new();
            segmentNames = new();
        }
    }
}