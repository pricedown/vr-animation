using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

// Written by Seth Riddensdale
namespace pricenerds3D
{
    /// <summary>
    ///     The data loader is split into two steps:
    ///     - LoadHTRData() - Loading HTR data and filling temporary data structures with any important information
    ///     - ProcessHTRData() - Processing HTR data, using the temporary data structures to create final structures to be used
    ///     by the programmer
    /// </summary>
    public static class P3D_DataLoader
    {
        /// <summary>
        ///     This is a static helper function that attempts to load our HTR data from the specified file path.
        ///     The function spits out a P3D_HTRDataContainer, returning true if it the loading succeeded and false if something
        ///     went wrong
        /// </summary>
        public static bool TryLoadHTRData(out P3D_HTRDataContainer data, string filePath)
        {
            data = new P3D_HTRDataContainer();

            // counters / helpers
            var segmentCounter = 0;
            var commentCounter = 0; // this will keep track if we are on the first / end comment for animation names
            var currentSegment = "";
            var currentSection = EHTRSection.HTR_File;

            // check if the file exists first
            if (!File.Exists(filePath))
            {
                Debug.LogError($"HTR file not found: {filePath}");
                return false;
            }

            // open and retrieve all lines from file
            var fileLines = File.ReadAllLines(filePath);

            // parse each line
            for (var i = 0; i < fileLines.Length; i++)
            {
                var rawLine = fileLines[i];
                var line = rawLine.Trim(); // remove white space

                // display the progress with a loading bar to show the user if its taking a while
                var progress = (float)i / fileLines.Length;
                EditorUtility.DisplayProgressBar("Reading HTR File", $"Parsed {i}/{fileLines.Length} lines  ",
                    progress);

                // if our current line is a comment, we may be on the start / end of an animation declaration
                if (line[0] == '#')
                {
                    // get the name of the animation
                    // is this the start of a new animation?
                    if (commentCounter == 0)
                    {
                        commentCounter =
                            1; // the comment counter will be 1, which means next time we hit a #, we know that we're at the end of the animation

                        // this logic takes place BEFORE we check which section we're in, so we need to ensure
                        // that we are either in the HTR_BasePose section or the HTR_NodePose section
                        if (currentSection == EHTRSection.HTR_BasePose || currentSection == EHTRSection.HTR_NodePose)
                        {
                            uint
                                indexModifier =
                                    2; // we start at 2 because the first line is the comment, and the second line is the segment name
                            uint frameCounter = 0;
                            var frameCountPreviewLine = fileLines[i + indexModifier];

                            // determine how many frames are in the current animation
                            while (frameCountPreviewLine[0] != '[')
                            {
                                // iterate counter
                                indexModifier++;
                                frameCountPreviewLine = fileLines[i + indexModifier];

                                // iterate frames counted
                                frameCounter++;
                            }

                            // Create new animation data and store it in the container
                            var animationName = line.Substring(1, line.Length - 1).Trim(' ');
                            var animData =
                                new P3D_HTRAnimationDataContainer(animationName, frameCounter);
                            data.animationData.Add(animData);
                        }
                    }
                    else
                    {
                        // Reset the comment counter to prepare to handle a new animation
                        commentCounter = 0;
                    }

                    continue;
                }

                // this line introduces a new section
                if (line[0] == '[')
                {
                    // remove square brackets from string
                    var sectionName = line.Substring(1, line.Length - 2);

                    // determine which section we are currently on
                    if (sectionName == sections[(int)EHTRSection.HTR_Header])
                    {
                        currentSection = EHTRSection.HTR_Header;
                    }
                    else if (sectionName == sections[(int)EHTRSection.HTR_Hierarchy])
                    {
                        currentSection = EHTRSection.HTR_Hierarchy;
                    }
                    else if (sectionName == sections[(int)EHTRSection.HTR_BasePose])
                    {
                        currentSection = EHTRSection.HTR_BasePose;
                    }
                    else if (sectionName == sections[(int)EHTRSection.HTR_EOF])
                    {
                        currentSection = EHTRSection.HTR_EOF;
                    }
                    // we'll need to read from our segment names here
                    else if (data.segmentNames != null && data.segmentNames.Contains(sectionName))
                    {
                        // might need to do something special here
                        currentSection = EHTRSection.HTR_NodePose;
                        currentSegment = sectionName;
                    }

                    // skip to the next line
                    continue;
                }

                // Add any relevant header information into the container
                if (currentSection == EHTRSection.HTR_Header)
                {
                    // splits the information into a string array
                    var headerStrArr = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    // Get segment count
                    if (headerStrArr[0] == headerComponents[(int)EHTRHeaderComponents.H_NumSegments])
                    {
                        data.numSegments = uint.Parse(headerStrArr[1]);
                    }
                    // Get frame count
                    else if (headerStrArr[0] == headerComponents[(int)EHTRHeaderComponents.H_NumFrames])
                    {
                        data.totalFrames = uint.Parse(headerStrArr[1]);
                    }
                    // Get frame rate
                    else if (headerStrArr[0] == headerComponents[(int)EHTRHeaderComponents.H_DataFrameRate])
                    {
                        data.frameRate = uint.Parse(headerStrArr[1]);
                    }
                    // Get euler rotation order
                    else if (headerStrArr[0] == headerComponents[(int)EHTRHeaderComponents.H_EulerRotationOrder])
                    {
                        data.eulerOrder = headerStrArr[1];
                    }
                    // Get scale factor
                    else if (headerStrArr[0] == headerComponents[(int)EHTRHeaderComponents.H_ScaleFactor])
                    {
                        data.scaleFactor = float.Parse(headerStrArr[1]);
                        data.globalScale = data.scaleFactor * (100.0f / 1000.0f); // match C implementation
                    }
                }

                // Add all segment hierarchy relationships in a dictionary to be processed later
                else if (currentSection == EHTRSection.HTR_Hierarchy)
                {
                    var hierarchyStrArr = line.Split('\t', ' ');

                    data.segmentNames.Add(hierarchyStrArr[0]); // add each segment
                    data.segmentHierarchy.Add(hierarchyStrArr[0], hierarchyStrArr[1]); // child, parent

                    segmentCounter++;
                }

                // reads all base pose data (each segments default position and orientation relative to the parent)
                // Segment      Tx Ty Tz    Rx Ry Rz Rw
                else if (currentSection == EHTRSection.HTR_BasePose)
                {
                    var baseStrArr = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    var segmentName = baseStrArr[0];

                    // add translation
                    var translation = new Vector3(
                        float.Parse(baseStrArr[1]) * data.globalScale,
                        float.Parse(baseStrArr[2]) * data.globalScale,
                        float.Parse(baseStrArr[3]) * data.globalScale);
                    data.basePosePosition.Add(segmentName, translation);

                    // add rotation
                    var eulerAngles = new Vector3(
                        float.Parse(baseStrArr[4]),
                        float.Parse(baseStrArr[5]),
                        float.Parse(baseStrArr[6]));
                    var rotation = EulerToQuaternion(eulerAngles, data.eulerOrder);
                    data.basePoseRotation.Add(segmentName, rotation);

                    // add scale
                    var jointScale = float.Parse(baseStrArr[7]);
                    data.basePoseScale.Add(segmentName, new Vector3(jointScale, jointScale, jointScale));
                }

                // read each nose
                // Frame num     Tx Ty Tz    Rx Ry Rz Rw
                else if (currentSection == EHTRSection.HTR_NodePose)
                {
                    var nodeStrArr = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var frameNum = uint.Parse(nodeStrArr[0]);

                    var position = new Vector3(
                        float.Parse(nodeStrArr[1]) * data.globalScale,
                        float.Parse(nodeStrArr[2]) * data.globalScale,
                        float.Parse(nodeStrArr[3]) * data.globalScale);

                    var eulerAngles = new Vector3(
                        float.Parse(nodeStrArr[4]),
                        float.Parse(nodeStrArr[5]),
                        float.Parse(nodeStrArr[6]));
                    var rotation = EulerToQuaternion(eulerAngles, data.eulerOrder);

                    var jointScale = float.Parse(nodeStrArr[7]);
                    var scale = new Vector3(jointScale, jointScale, jointScale);

                    var frameIndex = (int)frameNum - 1;
                    data.animationData[^1].position[frameIndex].Add(currentSegment, position);
                    data.animationData[^1].rotation[frameIndex].Add(currentSegment, rotation);
                    data.animationData[^1].scale[frameIndex].Add(currentSegment, scale);
                }
            }

            EditorUtility.ClearProgressBar();
            return true;
        }

        /// <summary>
        /// Converts euler angles to Quaternion based on rotation order
        /// </summary>
        /// <param name="euler">Euler angles</param>
        /// <param name="eulerOrder">Order by which axes are combined</param>
        /// <returns></returns>
        private static Quaternion EulerToQuaternion(Vector3 euler, string eulerOrder)
        {
            var qX = Quaternion.AngleAxis(euler.x, Vector3.right);
            var qY = Quaternion.AngleAxis(euler.y, Vector3.up);
            var qZ = Quaternion.AngleAxis(euler.z, Vector3.forward);

            switch (eulerOrder)
            {
                case "XYZ":
                    return qX * qY * qZ;
                case "XZY":
                    return qX * qZ * qY;
                case "YXZ":
                    return qY * qX * qZ;
                case "YZX":
                    return qY * qZ * qX;
                case "ZXY":
                    return qZ * qX * qY;
                case "ZYX":
                    return qZ * qY * qX;
                default:
                    Debug.LogWarning($"Unknown Euler order: {eulerOrder}");
                    return qX * qY * qZ;
            }
        }

        /// <summary>
        ///     This function is responsible for taking the data container from the loaded HTR file and actually processing it
        /// </summary>
        public static void ProcessHTRData(P3D_HTRDataContainer data, out P3D_ClipData[] clips, out P3D_Rig rig)
        {
            // initialize data
            clips = new P3D_ClipData[data.animationData.Count];
            rig = new P3D_Rig((uint)data.segmentHierarchy.Count);
            BuildHierarchy(data, rig);

            // iterate through each animation
            for (var animationIndex = 0; animationIndex < data.animationData.Count; animationIndex++)
            {
                // create new clip data
                var clipData = ScriptableObject.CreateInstance<P3D_ClipData>();
                clipData.clipName = data.animationData[animationIndex].animationName;

                var animData = data.animationData[animationIndex];
                var sampleCount = animData.numFrames;

                // create a new clip and store it in clip data scriptable object
                var jointCount = (uint)data.segmentNames.Count;
                var clip = new P3D_Clip(sampleCount, jointCount);

                var playbackRate = data.frameRate > 0 ? data.frameRate : 30f;
                var keyframeDuration = 1f / playbackRate;
                var totalDuration = (sampleCount - 1) * keyframeDuration;

                clip.SetDuration(totalDuration);

                for (var kf = 0; kf < clip.keyframes.Length; kf++)
                    clip.SetKeyframeDuration(kf, keyframeDuration);

                for (uint frameIdx = 0; frameIdx < sampleCount; frameIdx++)
                {
                    var sample = clip.samples[frameIdx];

                    for (var jointIdx = 0; jointIdx < data.segmentNames.Count; jointIdx++)
                    {
                        var segmentName = data.segmentNames[jointIdx];
                        var pos = Vector3.zero;
                        var rot = Quaternion.identity;
                        var scl = Vector3.one;

                        if (animData.position[(int)frameIdx].TryGetValue(segmentName, out var p)) pos = p;
                        if (animData.rotation[(int)frameIdx].TryGetValue(segmentName, out var r)) rot = r;
                        if (animData.scale[(int)frameIdx].TryGetValue(segmentName, out var s)) scl = s;

                        sample.SetJointPose(jointIdx, pos, rot, scl);
                    }
                }

                for (uint frameIndex = 0; frameIndex < data.animationData[animationIndex].numFrames; frameIndex++)
                {
                    // For each segment set the sample data
                }

                // the HTR file contains the offsets from the base pose
                // for every segment
                /*
                for(int segmentIndex = 0; segmentIndex < data.segmentNames.Count; segmentIndex++)
                {
                    string currentSegmentName = data.segmentNames[segmentIndex];
                    // create first keyframe, then start from the second keyframe and move forward to access its end sample
                    P3D_Keyframe keyframe0 = new P3D_Keyframe();
                    P3D_Sample sample0 = new P3D_Sample(0);
                    sample0.localTranslation = data.animationData[animationIndex].position[0][currentSegmentName];
                    sample0.localRotation = data.animationData[animationIndex].rotation[];

                    P3D_Sample sample1 = new P3D_Sample(1);

                    for (int frameIndex = 1; frameIndex < data.totalFrames; frameIndex++)
                    {
                        // we need to access the end sample of the previous and create a new sample for the next

                    }
                }*/

                clipData.clip = clip;
                clips[animationIndex] = clipData;

                /*
                Debug.Log($"Animation name: {data.animationData[i].animationName}");
                Debug.Log($"Frame count: {data.animationData[i].numFrames}");
                Debug.Log($"Positions count: {data.animationData[i].position.Length}");
                Debug.Log($"Rotations count: {data.animationData[i].rotation.Length}");*/
            }
        }

        /// <summary>
        ///     Helper function that builds a hierarchy from the string list and dictionary harvested from the HTR file. creates a
        ///     rig
        /// </summary>
        private static void BuildHierarchy(P3D_HTRDataContainer data, P3D_Rig rig)
        {
            for (var i = 0; i < data.segmentNames.Count; i++)
            {
                var segmentName = data.segmentNames[i];
                rig.m_joints[i].m_name = segmentName;
                rig.m_joints[i].m_jointIndex = -1;

                int parentIndex;
                // this would be the root, which has no parent index
                if (data.segmentHierarchy[segmentName] == "GLOBAL") parentIndex = -1;
                // otherwise, find the index of the parent in the segment names
                else parentIndex = data.segmentNames.IndexOf(data.segmentHierarchy[segmentName]);

                rig.m_joints[i].m_parentIndex = parentIndex;

                if (data.basePosePosition.ContainsKey(segmentName))
                    rig.m_joints[i].m_localPosition = data.basePosePosition[segmentName];
                if (data.basePoseRotation.ContainsKey(segmentName))
                    rig.m_joints[i].m_localRotation = data.basePoseRotation[segmentName];
                if (data.basePoseScale.ContainsKey(segmentName))
                    rig.m_joints[i].m_localScale = data.basePoseScale[segmentName];
            }
        }

        #region Enum / String definitions

        public enum EHTRSection
        {
            HTR_File,
            HTR_Header,
            HTR_Hierarchy,
            HTR_BasePose,
            HTR_NodePose,
            HTR_EOF
        }

        public enum EHTRHeaderComponents
        {
            H_FileType,
            H_DataType,
            H_FileVersion,
            H_NumSegments,
            H_NumFrames,
            H_DataFrameRate,
            H_EulerRotationOrder,
            H_CalibrationUnits,
            H_RotationUnits,
            H_GlobalAxisofGravity,
            H_BoneLengthAxis,
            H_ScaleFactor
        }

        // An ordered string array that corresponds to each EHTRSection enum value
        // Empty "" in this array are sections that our loader doesn't need to read from
        private static readonly string[] sections =
        {
            "",
            "Header",
            "SegmentNames&Hierarchy",
            "BasePosition",
            "",
            "EndOfFile"
        };

        // An ordered string array that corresponds to each EHTRHeaderComponents enum value
        private static readonly string[] headerComponents =
        {
            "FileType",
            "DataType",
            "FileVersion",
            "NumSegments",
            "NumFrames",
            "DataFrameRate",
            "EulerRotationOrder",
            "CalibrationUnits",
            "RotationUnits",
            "GlobalAxisofGravity",
            "BoneLengthAxis",
            "ScaleFactor"
        };

        #endregion
    }
}