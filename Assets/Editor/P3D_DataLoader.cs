using UnityEngine;
using System.IO;
using Debug = UnityEngine.Debug;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

// Written by Seth Riddensdale
namespace pricenerds3D
{
    /// <summary>
    /// The data loader is split into two steps:
    /// - LoadHTRData() - Loading HTR data and filling temporary data structures with any important information
    /// - ProcessHTRData() - Processing HTR data, using the temporary data structures to create final structures to be used by the programmer
    /// </summary>
    public static class P3D_DataLoader
    {
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
        private static string[] sections = new string[] { 
            "",
            "Header", 
            "SegmentNames&Hierarchy", 
            "BasePosition", 
            "",
            "EndOfFile" 
        };

        // An ordered string array that corresponds to each EHTRHeaderComponents enum value
        private static string[] headerComponents = new string[] { 
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
            "ScaleFactor",
        };
        #endregion

        /// <summary>
        /// This is a static helper function that attempts to load our HTR data from the specified file path. 
        /// The function spits out a P3D_HTRDataContainer, returning true if it the loading succeeded and false if something went wrong
        /// </summary>
        public static bool TryLoadHTRData(out P3D_HTRDataContainer data, string filePath)
        {
            data = new P3D_HTRDataContainer();

            // counters / helpers
            int segmentCounter = 0;
            int commentCounter = 0; // this will keep track if we are on the first / end comment for animation names
            string currentSegment = "";
            EHTRSection currentSection = EHTRSection.HTR_File;

            // check if the file exists first
            if (!File.Exists(filePath))
            {
                Debug.LogError($"HTR file not found: {filePath}");
                return false;
            }

            // open and retrieve all lines from file
            string[] fileLines = File.ReadAllLines(filePath);

            // parse each line
            for(int i = 0; i < fileLines.Length; i++)
            {
                string rawLine = fileLines[i];
                string line = rawLine.Trim(); // remove white space

                // display the progress with a loading bar to show the user if its taking a while
                float progress = (float)i / fileLines.Length;
                EditorUtility.DisplayProgressBar($"Reading HTR File", $"Parsed {i}/{fileLines.Length} lines  ", progress);

                // if our current line is a comment, we may be on the start / end of an animation declaration
                if (line[0] == '#')
                {
                    // get the name of the animation
                    // is this the start of a new animation?
                    if (commentCounter == 0)
                    {
                        commentCounter = 1; // the comment counter will be 1, which means next time we hit a #, we know that we're at the end of the animation

                        // this logic takes place BEFORE we check which section we're in, so we need to ensure
                        // that we are either in the HTR_BasePose section or the HTR_NodePose section
                        if (currentSection == EHTRSection.HTR_BasePose || currentSection == EHTRSection.HTR_NodePose)
                        {
                            uint indexModifier = 2; // we start at 2 because the first line is the comment, and the second line is the segment name
                            uint frameCounter = 0;
                            string frameCountPreviewLine = fileLines[i + indexModifier];

                            // determine how many frames are in the current animation
                            while (frameCountPreviewLine[0] != '[')
                            {
                                // iterate counter
                                indexModifier++;
                                frameCountPreviewLine = fileLines[i + indexModifier];

                                // iterate frames counted
                                frameCounter++;
                            }

                            // get the animation name
                            string animationName = line.Substring(1, line.Length - 1).Trim(' ');
                            P3D_HTRAnimationDataContainer animData = new P3D_HTRAnimationDataContainer(animationName, frameCounter);
                            data.animationData.Add(animData);
                        }
                    }
                    else
                    {
                        commentCounter = 0;
                    }

                    continue;
                }

                // line introduces a new section
                if (line[0] == '[')
                {
                    // remove square brackets
                    string sectionName = line.Substring(1, line.Length - 2);

                    if (sectionName == sections[(int)EHTRSection.HTR_Header])
                        currentSection = EHTRSection.HTR_Header;
                    else if (sectionName == sections[(int)EHTRSection.HTR_Hierarchy])
                        currentSection = EHTRSection.HTR_Hierarchy;
                    else if (sectionName == sections[(int)EHTRSection.HTR_BasePose])
                        currentSection = EHTRSection.HTR_BasePose;
                    else if (sectionName == sections[(int)EHTRSection.HTR_EOF])
                    {
                        // might need to do something special here
                        currentSection = EHTRSection.HTR_EOF;
                    }
                    // we'll need to read from our segment names here
                    else if (data.segmentNames != null && data.segmentNames.Contains(sectionName))
                    {
                        // might need to do something special here
                        currentSection = EHTRSection.HTR_NodePose;
                        currentSegment = sectionName;
                    }

                    // skip if we're on a section
                    continue;
                }

                // Add any relevant header information into the container
                if (currentSection == EHTRSection.HTR_Header)
                {
                    string[] headerStrArr = line.Split('\t', ' '); // splits the information into a string array

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
                }

                // Add all segment hierarchy relationships in a dictionary to be processed later
                else if (currentSection == EHTRSection.HTR_Hierarchy)
                {
                    string[] hierarchyStrArr = line.Split('\t', ' ');

                    data.segmentNames.Add(hierarchyStrArr[0]); // add each segment
                    data.segmentHierarchy.Add(hierarchyStrArr[0], hierarchyStrArr[1]); // child, parent

                    segmentCounter++;
                }

                // reads all base pose data (each segments default position and orientation relative to the parent)
                // Segment      Tx Ty Tz    Rx Ry Rz Rw
                else if (currentSection == EHTRSection.HTR_BasePose)
                {
                    string[] baseStrArr = line.Split('\t', ' ');

                    // add translation
                    data.basePosePosition.Add(baseStrArr[0], new Vector3(
                        float.Parse(baseStrArr[1]),
                        float.Parse(baseStrArr[2]),
                        float.Parse(baseStrArr[3])));

                    // add rotation
                    data.basePoseRotation.Add(baseStrArr[0], new Quaternion(
                        float.Parse(baseStrArr[4]),
                        float.Parse(baseStrArr[5]),
                        float.Parse(baseStrArr[6]),
                        float.Parse(baseStrArr[7])));
                }

                // read each nose
                // Frame num     Tx Ty Tz    Rx Ry Rz Rw
                else if (currentSection == EHTRSection.HTR_NodePose)
                {
                    string[] nodeStrArr = line.Split('\t', ' ');
                    uint frameNum = uint.Parse(nodeStrArr[0]);

                    Vector3 position = new Vector3(
                        float.Parse(nodeStrArr[1]),
                        float.Parse(nodeStrArr[2]),
                        float.Parse(nodeStrArr[3]));

                    Quaternion rotation = new Quaternion(
                        float.Parse(nodeStrArr[4]),
                        float.Parse(nodeStrArr[5]),
                        float.Parse(nodeStrArr[6]),
                        float.Parse(nodeStrArr[7]));

                    data.animationData[data.animationData.Count - 1].position[frameNum - 1].Add(currentSegment, position);
                    data.animationData[data.animationData.Count - 1].rotation[frameNum - 1].Add(currentSegment, rotation);
                }
            }
            EditorUtility.ClearProgressBar();
            return true;
        }

        public static void ProcessHTRData(P3D_HTRDataContainer data, out P3D_ClipData[] clips, out P3D_Skeleton skeleton)
        {
            // initialize data
            clips = new P3D_ClipData[data.animationData.Count];
            skeleton = new P3D_Skeleton((uint)data.segmentHierarchy.Count);

            BuildHierarchy(data.segmentNames, data.segmentHierarchy, skeleton);

            for (int i = 0; i < data.animationData.Count; i++) 
            {
                // process each pose
               
                // create new clip
                clips[i] = new P3D_ClipData(data.animationData[i].animationName);

                Debug.Log($"Animation name: {data.animationData[i].animationName}");
                Debug.Log($"Frame count: {data.animationData[i].numFrames}");
                Debug.Log($"Positions count: {data.animationData[i].position.Length}");
                Debug.Log($"Rotations count: {data.animationData[i].rotation.Length}");
            }
        }

        private static void BuildHierarchy(List<string> segmentNames, Dictionary<string, string> segmentHierarchy, P3D_Skeleton skeleton)
        {
            for (int i = 0; i < segmentNames.Count; i++)
            {
                skeleton.m_joints[i].m_name = segmentNames[i];
                sbyte parentIndex;

                // this would be the root, which has no parent index
                if (segmentHierarchy[segmentNames[i]] == "GLOBAL") parentIndex = -1;
                // otherwise, find the index of the parent in the segment names
                else parentIndex = (sbyte)segmentNames.IndexOf(segmentHierarchy[segmentNames[i]]);

                skeleton.m_joints[i].m_parentIndex = parentIndex;
            }
        }
    }
}