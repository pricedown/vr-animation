using UnityEngine;
using System.IO;
using Debug = UnityEngine.Debug;

namespace pricenerds3D
{
    // Based on Animal3D by Dan Buckstein
    public static class P3D_DataLoader
    {
        private enum EHTRSection
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

            // does the file exist?
            if (!File.Exists(filePath))
            {
                Debug.LogError($"HTR file not found: {filePath}");
                return false;
            }

            // open and retrieve all lines from file
            string[] fileLines = File.ReadAllLines(filePath);

            return true;
        }
    }
}