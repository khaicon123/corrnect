using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Handles level saving and loading
/// </summary>
public class SaveLoadSystem
{
    private LevelEditorState editorState;
    private const string LEVELS_FOLDER = "Assets/GamePlay/Levels";

    public SaveLoadSystem(LevelEditorState state)
    {
        editorState = state;
        EnsureLevelsFolder();
    }

    public void QuickSave()
    {
        if (editorState.CurrentLevel == null)
        {
            EditorUtility.DisplayDialog("Error", "No level to save", "OK");
            return;
        }

        SaveLevel(editorState.CurrentLevel);
    }

    public void ShowLoadDialog()
    {
        string path = EditorUtility.OpenFilePanel("Load Level", LEVELS_FOLDER, "json");
        if (!string.IsNullOrEmpty(path))
        {
            LoadLevelFromPath(path);
        }
    }

    public void SaveLevel(LevelData level)
    {
        if (level == null)
            return;

        EnsureLevelsFolder();
        string filePath = Path.Combine(LEVELS_FOLDER, $"{level.LevelName}.json");

        string json = JsonUtility.ToJson(level, true);
        File.WriteAllText(filePath, json);

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", $"Level saved: {filePath}", "OK");
    }

    public void LoadLevelFromPath(string filePath)
    {
        if (!File.Exists(filePath))
        {
            EditorUtility.DisplayDialog("Error", "File not found", "OK");
            return;
        }

        string json = File.ReadAllText(filePath);
        
        try
        {
            LevelData loadedLevel = JsonUtility.FromJson<LevelData>(json);
            editorState.CurrentLevel = loadedLevel;
            editorState.SaveState();
            EditorUtility.DisplayDialog("Success", $"Level loaded: {loadedLevel.LevelName}", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to load level:\n{e.Message}", "OK");
        }
    }

    public void ExportAsScriptableObject()
    {
        if (editorState.CurrentLevel == null)
        {
            EditorUtility.DisplayDialog("Error", "No level to export", "OK");
            return;
        }

        string path = EditorUtility.SaveFilePanelInProject(
            "Export Level as ScriptableObject",
            editorState.CurrentLevel.LevelName,
            "asset",
            "Export level"
        );

        if (string.IsNullOrEmpty(path))
            return;

        LevelDataAsset asset = ScriptableObject.CreateInstance<LevelDataAsset>();
        asset.levelData = editorState.CurrentLevel;

        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", $"Level exported to:\n{path}", "OK");
    }

    private void EnsureLevelsFolder()
    {
        if (!Directory.Exists(LEVELS_FOLDER))
        {
            Directory.CreateDirectory(LEVELS_FOLDER);
            AssetDatabase.Refresh();
        }
    }
}

/// <summary>
/// ScriptableObject wrapper for level data
/// </summary>
public class LevelDataAsset : ScriptableObject
{
    public LevelData levelData;
}
