using UnityEngine;
using System.IO;
using Debug = UnityEngine.Debug;
using UnityEditor;
using System.Collections.Generic;

namespace pricenerds3D
{
    // Based on Animal3D by Dan Buckstein

    // this data container will be filled with all the data we load from the HTR file
    public struct P3D_HTRDataContainer
    {
        public uint numSegments;
        public uint numFrames;
        public uint frameRate;

        public Dictionary<string, string> segmentHierarchy;
        public Dictionary<string, Vector3> basePosePosition;
        public Dictionary<string, Quaternion> basePoseRotation;
    }

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
            data.segmentHierarchy = new();
            data.basePosePosition = new();
            data.basePoseRotation = new();

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
                    else if (sectionName == sections[(int)EHTRSection.HTR_NodePose])
                    {
                        // might need to do something special here
                        currentSection = EHTRSection.HTR_NodePose;
                    }

                    // skip if we're on a section
                    continue;
                }

                // Add any relevant header information into the container
                if(currentSection == EHTRSection.HTR_Header)
                {
                    string[] parts = line.Split('\t', ' '); // splits the information into a string array

                    Debug.Log($"{parts[0]}, {headerComponents[(int)EHTRHeaderComponents.H_NumSegments]}");

                    if (parts[0] == headerComponents[(int)EHTRHeaderComponents.H_NumSegments]) data.numSegments = uint.Parse(parts[1]);
                    else if (parts[0] == headerComponents[(int)EHTRHeaderComponents.H_NumFrames]) data.numFrames = uint.Parse(parts[1]);
                    else if (parts[0] == headerComponents[(int)EHTRHeaderComponents.H_DataFrameRate]) data.frameRate = uint.Parse(parts[1]);
                }

                // Add all segment hierarchy relationships in a dictionary to be processed later
                if (currentSection == EHTRSection.HTR_Hierarchy)
                {
                    string[] parts = line.Split('\t', ' '); // splits the information into a string array
                    data.segmentHierarchy.Add(parts[0], parts[1]);
                }
            }
            EditorUtility.ClearProgressBar();
            return true;
        }

        public static void ProcessHTRData(P3D_HTRDataContainer data, out P3D_SkeletonPose[] poses, out P3D_Skeleton skeleton)
        {
            poses = null;
            skeleton = null;
        }
    }
}