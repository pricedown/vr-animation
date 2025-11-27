using UnityEngine;
using System.IO;
using Debug = UnityEngine.Debug;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace pricenerds3D
{
    // Based on Animal3D by Dan Buckstein

    // this data container will be filled with all the data we load from the HTR file
    public class P3D_HTRDataContainer
    {
        public uint numSegments;
        public uint totalFrames;
        public uint frameRate;

        public List<string> segmentNames;
        public Dictionary<string, string> segmentHierarchy;
        public Dictionary<string, Vector3> basePosePosition;
        public Dictionary<string, Quaternion> basePoseRotation;
        public List<P3D_HTRAnimationDataContainer> animationData;

        public P3D_HTRDataContainer()
        {
            segmentHierarchy = new();
            basePosePosition = new();
            basePoseRotation = new();
            animationData = new();
            segmentNames = new();
        }
    }

    // load each animation into seperate scriptable object data
    public class P3D_HTRAnimationDataContainer
    {
        public uint numFrames;
        public string animationName;

        public Dictionary<string, Vector3>[] position;
        public Dictionary<string, Quaternion>[] rotation;

        public P3D_HTRAnimationDataContainer(string animationName, uint numFrames)
        {
            this.numFrames = numFrames;
            this.animationName = animationName;

            position = new Dictionary<string, Vector3>[numFrames];
            rotation = new Dictionary<string, Quaternion>[numFrames];

            // initialize each dictionary
            for (int i = 0; i < numFrames; i++)
            {
                position[i] = new Dictionary<string, Vector3>();
                rotation[i] = new Dictionary<string, Quaternion>();
            }
        }
    }

    // algorithm
    // 1. new animation? check how many frames are in the animation
    // 2. fill in position and rotation for each joint

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

        private static string[] sections = new string[] { 
            "",
            "Header", 
            "SegmentNames&Hierarchy", 
            "BasePosition", 
            "",
            "EndOfFile" 
        };

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

        public static bool LoadHTRData(out P3D_HTRDataContainer data, string filePath)
        {
            data = new P3D_HTRDataContainer();

            // counters / helpers
            int segmentCounter = 0;
            int commentCounter = 0; // this will keep track if we are on the first / end comment for animation names
            string currentSegment = "";

            EHTRSection currentSection = EHTRSection.HTR_File;

            // does the file exist?
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
                string line = rawLine.Trim(); // trims any white space from the line
                float progress = (float)i / fileLines.Length;
                EditorUtility.DisplayProgressBar($"Reading HTR File", $"Parsed {i}/{fileLines.Length} lines  ", progress);

                if (line[0] == '#')
                {
                    string name = line.Substring(1).Trim();

                    // is this starting a new animation?
                    if (commentCounter == 0)
                    {
                        commentCounter = 1;

                        // the next section will be a node pose
                        if (currentSection == EHTRSection.HTR_BasePose || currentSection == EHTRSection.HTR_NodePose)
                        {
                            uint indexModifier = 2; // we start at 2 because #, and [
                            uint frameCounter = 0;
                            string frameCountPreviewLine = fileLines[i + indexModifier];

                            // go until end of animation
                            while (frameCountPreviewLine[0] != '[')
                            {
                                // iterate counter
                                indexModifier++;
                                frameCountPreviewLine = fileLines[i + indexModifier];

                                // iterate frames counted
                                frameCounter++;
                            }

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