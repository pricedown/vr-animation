using System.Collections.Generic;
using UnityEngine;

// Written by Seth Riddensdale
namespace pricenerds3D
{
    /// <summary>
    ///     This is a data container that contains all important data loaded from an HTR file
    /// </summary>
    public class P3D_HTRDataContainer
    {
        public List<P3D_HTRAnimationDataContainer> animationData; // list of each animation processed by the htr loader

        public Dictionary<string, Vector3> basePosePosition; // child (string) -> position (vector3)
        public Dictionary<string, Quaternion> basePoseRotation; // child (string) -> rotation (quaternion)
        public Dictionary<string, Vector3> basePoseScale; // child (string) -> scale (vector3)
        public string eulerOrder = "XYZ";
        public uint frameRate; // useful comment about frame rate
        public float globalScale = 1.0f;
        public uint numSegments; // the amount of bones in our hierarchy
        public float scaleFactor = 1.0f;
        public Dictionary<string, string> segmentHierarchy; // child (string) -> parent (string)

        public List<string> segmentNames;
        public uint totalFrames; // the total frames of all animations in the file

        public P3D_HTRDataContainer()
        {
            // constructor initializes all of our structures
            segmentHierarchy = new Dictionary<string, string>();
            basePosePosition = new Dictionary<string, Vector3>();
            basePoseRotation = new Dictionary<string, Quaternion>();
            basePoseScale = new Dictionary<string, Vector3>();
            animationData = new List<P3D_HTRAnimationDataContainer>();
            segmentNames = new List<string>();
        }
    }
}