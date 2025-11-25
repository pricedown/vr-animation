using UnityEngine;
using System.IO;
using Debug = UnityEngine.Debug;
using UnityEditor;

namespace pricenerds3D
{
    // Based on Animal3D by Dan Buckstein
    public static class P3D_DataLoader
    {
        public enum EHTRSection
        {
            HTR_File,
            HTR_Header,
            HTR_Hierarchy,
            HTR_BasePose,
            HTR_NodePose,
            HTR_EOF
        }

        private static string[] sections = new string[] { 
            "",
            "[Header]", 
            "[SegmentNames&Hierarchy]", 
            "[BasePosition]", 
            "",
            "[EndOfFile]" 
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

        // this might actually need to load a bunch of poses but im still figuring that out
        public static bool TryLoadHTR(out P3D_SkeletonPose pose, out P3D_Skeleton skeleton, string filePath)
        {
            // the HTR file contains multiple sections: header, segment names & hierarchy, base position and motion sections

            // FileType - describes the type of file
            // DataType - describes the transformation composition of translation, rotation, and scale
            // FileVersion - describes the file version
            // NumSegments - the count of segments found in the file
            // NumFrames - the number of frames (samples) in the file
            // CalibrationUnits - the translation units for the file
            // RotationUnits - the rotation units for the file (almost always degrees)
            // GlobalAxisofGravity - specifies the global up axis of the data (positive Y is the default, positive Z is a common alternative)
            // BoneLengthAxis - the axis along which each segment is aligned, assume default is the Y axis
            // ScaleFactor - the global scale applied to the data

            // interpreting the data
            // 1. create a matrix of rotation data from the motion data (matrix A)
            // 2. take the initial rotation matrix (matrix B) multiply A on the right by B
            // 3. sum the translation motion data and the initial motion data to create another matrix C, multiply AB on the right by C
            // 4. the resulting transform, ABC, is the local transform for the element. multiply this on the right by the parents local transform, etc

            pose = null;
            skeleton = null;
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
                }

                switch (currentSection)
                {
                    case EHTRSection.HTR_Header:
                        break;
                    case EHTRSection.HTR_Hierarchy:
                        break;
                    case EHTRSection.HTR_BasePose:
                        break;
                    case EHTRSection.HTR_NodePose:
                        break;
                }
            }
            EditorUtility.ClearProgressBar();

            return true;
        }
    }
}