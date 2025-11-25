using UnityEngine;
using UnityEditor;
using pricenerds3D;
using Unity.Tutorials.Core.Editor;

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

    private void OnDestroy()
    {
        ResetWindow();
    }

    private static void ResetWindow()
    {
        currentSelectedFilePath = "";
        hasSelectedFilePath = false;
    }

    private void OnGUI()
    {
        if(GUILayout.Button("Select File"))
        {
            currentSelectedFilePath = EditorUtility.OpenFilePanelWithFilters("Select File", "Assets", new string[] { "FBX", "fbx" });
        }

        if(!currentSelectedFilePath.IsNullOrEmpty())
        {
            hasSelectedFilePath = true;

            GUILayout.BeginHorizontal();
            GUILayout.Label("File Selected: ");
            GUILayout.Label(currentSelectedFilePath, EditorStyles.boldLabel);
            GUILayout.EndHorizontal();
        }
        else
        {
            hasSelectedFilePath = false;
        }

        if (hasSelectedFilePath)
        {
            if (GUILayout.Button("Build Skeleton"))
            {
                string savePath = EditorUtility.SaveFilePanel("Select Directory", "Assets", "Skeleton", "asset");
                string fileName = System.IO.Path.GetFileName(savePath);

                // determine relative path to project
                string projectPath = Application.dataPath;
                projectPath = projectPath.Replace("/Assets", "");

                string relativePath = "";
                if (savePath.StartsWith(projectPath)) relativePath = savePath.Substring(projectPath.Length + 1);
                // error handling if no relative path
                else
                {
                    Debug.LogError($"{SAVE_FAILURE_ERROR_LOG} Project path is outside of the scope of the project!");
                    return;
                }

                P3D_GeneratedSkeletonAsset skeletonAsset = ScriptableObject.CreateInstance<P3D_GeneratedSkeletonAsset>();
                skeletonAsset.name = fileName;

                // create the file
                AssetDatabase.CreateAsset(skeletonAsset, relativePath);
                AssetDatabase.SaveAssets();

                // update the project window
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = skeletonAsset;
                Debug.Log($"{SAVE_SUCCESS_LOG}");
            }
        }
    }
}
