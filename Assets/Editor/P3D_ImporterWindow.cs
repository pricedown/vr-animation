using UnityEngine;
using UnityEditor;
using Unity.Tutorials.Core.Editor;

// Written by Seth Riddensdale
namespace pricenerds3D
{
    /// <summary>
    /// This class is an editor only class that interfaces with our HTR loader to allow the user to import a file
    /// Generates a prefab for our rig instance, aswell as a ScriptableObject container for every AnimationClip in the HTR file
    /// </summary>
    public class P3D_ImporterWindow : EditorWindow
    {
        private const string SAVE_FAILURE_ERROR_LOG = "Failed to save file!";
        private const string SAVE_SUCCESS_LOG = "Sucessfully saved file.";

        private static string currentSelectedFilePath;
        private static bool hasSelectedFilePath;

        [MenuItem("Window/P3D Importer")]
        public static void ShowWindow()
        {
            GetWindow<P3D_ImporterWindow>("P3D Importer");
            ResetWindow();
        }

        private void OnGUI()
        {
            GetFilePath();

            if (hasSelectedFilePath && GUILayout.Button("Build Skeleton"))
            {
                OnBuildPressed();
            }
        }

        private void OnDestroy()
        {
            ResetWindow();
        }

        #region Helpers
        private void OnBuildPressed()
        {
            string savePath = EditorUtility.SaveFilePanel("Select Directory", "Assets", System.IO.Path.GetFileNameWithoutExtension(currentSelectedFilePath), "asset");
            if (!TryGetSkeletonGenerationPath(savePath, out string path)) return;
            if (!P3D_DataLoader.TryLoadHTRData(out P3D_HTRDataContainer data, currentSelectedFilePath)) return;

            // process data and send it to the generation step
            P3D_DataLoader.ProcessHTRData(data, out P3D_ClipData[] clipData, out P3D_Skeleton skeletonData);
            GenerateAnimationFiles(path, clipData, skeletonData);

            Debug.Log($"{SAVE_SUCCESS_LOG}");
        }

        private static void GetFilePath()
        {
            // Display a button to select a file
            if (GUILayout.Button("Select File"))
            {
                currentSelectedFilePath = EditorUtility.OpenFilePanelWithFilters("Select File", "Assets", new string[] { "HTR", "htr" });
            }

            // If the file path is filled, we can proceed to the next step
            if (!currentSelectedFilePath.IsNullOrEmpty())
            {
                hasSelectedFilePath = true;
                DrawFilePathLabels(currentSelectedFilePath);
            }
            // Otherwise, file path has not been specified
            else
            {
                hasSelectedFilePath = false;
            }
        }

        private static void ResetWindow()
        {
            currentSelectedFilePath = "";
            hasSelectedFilePath = false;
        }

        private static void DrawFilePathLabels(string filePath)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("File Selected: ", GUILayout.ExpandWidth(false));
            GUILayout.Label(currentSelectedFilePath, EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }

        // This function determines a relative path to the inPath. UnityEditor doesn't like full paths, it wants a path relative to the project folder
        private static bool TryGetSkeletonGenerationPath(string inPath, out string savePath)
        {
            string projectPath = Application.dataPath;
            savePath = inPath;
            projectPath = projectPath.Replace("/Assets", "");

            // Check if this is a valid relative path
            if (savePath.StartsWith(projectPath))
            {
                savePath = savePath.Substring(projectPath.Length + 1);
                return true;
            }

            // If we still have content in the string and it is not a relative path, something is wrong with the specified path
            else if (!savePath.IsNullOrEmpty())
            {
                Debug.LogError($"{SAVE_FAILURE_ERROR_LOG} Project path is outside of the scope of the project!");
                return false;
            }

            // In the event that the user presses cancel, just return false (the only other possible case)
            return false;
        }

        // Helper function that generates a skeleton data object
        private static void GenerateAnimationFiles(string path, P3D_ClipData[] clipData, P3D_Skeleton skeletonData)
        {
            // create asset instance to written to the disk
            P3D_GeneratedSkeletonAsset skeletonAsset = CreateInstance<P3D_GeneratedSkeletonAsset>();
            skeletonAsset.name = System.IO.Path.GetFileName(path);
            skeletonAsset.m_generatedSkeleton = skeletonData;

            // create the file
            AssetDatabase.CreateAsset(skeletonAsset, path);
            AssetDatabase.SaveAssets();

            // update the project window
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = skeletonAsset;

            string nameWithoutExtention = System.IO.Path.GetFileNameWithoutExtension(path);
            string localPath = System.IO.Path.GetDirectoryName(path) + "/" + System.IO.Path.GetFileNameWithoutExtension(path) + ".prefab";

            // create prefab
            GameObject blueprint = CreateSkeletonPrefabBlueprint(nameWithoutExtention, skeletonAsset);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(blueprint, localPath, out bool success);
            DestroyImmediate(blueprint);

            if (!success) Debug.LogError("Failed to create prefab!");

            Debug.LogWarning("poseData is an unused parameter that is not set yet, please do that !!!!!");
        }

        private static GameObject CreateSkeletonPrefabBlueprint(string name, P3D_GeneratedSkeletonAsset data)
        {
            // create a new gameobject
            GameObject blueprint = new GameObject();
            blueprint.name = name;

            // add a rig component
            P3D_RigInstance rig = blueprint.AddComponent<P3D_RigInstance>();
            rig.data = data;

            return blueprint;
        }
        #endregion
    }
}