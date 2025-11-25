using UnityEngine;
using UnityEditor;
using pricenerds3D;
using Unity.Tutorials.Core.Editor;
using UnityEditor.Overlays;

public class P3D_ImporterWindow : EditorWindow
{
    private static string currentSelectedFilePath;
    private static bool hasSelectedFilePath;

    private const string SAVE_FAILURE_ERROR_LOG = "Failed to save file!";
    private const string SAVE_SUCCESS_LOG = "Sucessfully saved file.";

    [MenuItem("Window/P3D Importer")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<P3D_ImporterWindow>("P3D Importer");
        ResetWindow();
    }

    private void OnGUI()
    {
        GetFilePath();

        if (hasSelectedFilePath)
        {
            if (GUILayout.Button("Build Skeleton"))
            {
                string savePath = EditorUtility.SaveFilePanel("Select Directory", "Assets", "Skeleton", "asset");

                if (TryGetSkeletonGenerationPath(savePath, out string path))
                {
                    GenerateSkeletonFile(path);
                    Debug.Log($"{SAVE_SUCCESS_LOG}");
                }
            }
        }
    }

    private void OnDestroy()
    {
        ResetWindow();
    }

    private static void GetFilePath()
    {
        // Display a button to select a file
        if (GUILayout.Button("Select File"))
        {
            currentSelectedFilePath = EditorUtility.OpenFilePanelWithFilters("Select File", "Assets", new string[] { "FBX", "fbx" });
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
        string relativePath = "";

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
    private static void GenerateSkeletonFile(string path)
    {
        P3D_GeneratedSkeletonAsset skeletonAsset = ScriptableObject.CreateInstance<P3D_GeneratedSkeletonAsset>();
        skeletonAsset.name = System.IO.Path.GetFileName(path);

        // create the file
        AssetDatabase.CreateAsset(skeletonAsset, path);
        AssetDatabase.SaveAssets();

        // update the project window
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = skeletonAsset;
    }
}
