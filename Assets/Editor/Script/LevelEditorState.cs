using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Stores the state of the Level Editor
/// </summary>
public class LevelEditorState
{
    public LevelData CurrentLevel { get; set; }
    public GridSettings GridSettings { get; set; }
    public List<TileData> SelectedTiles { get; set; }
    public PreviewSettings PreviewSettings { get; set; }

    private const string STATE_KEY = "LevelEditorState_";

    public LevelEditorState()
    {
        GridSettings = new GridSettings();
        SelectedTiles = new List<TileData>();
        PreviewSettings = new PreviewSettings();
        RestoreState();
    }

    public void SaveState()
    {
        if (CurrentLevel != null)
        {
            EditorPrefs.SetString(STATE_KEY + "CurrentLevel", JsonUtility.ToJson(CurrentLevel));
        }
        EditorPrefs.SetString(STATE_KEY + "GridSettings", JsonUtility.ToJson(GridSettings));
        EditorPrefs.SetString(STATE_KEY + "PreviewSettings", JsonUtility.ToJson(PreviewSettings));
    }

    public void RestoreState()
    {
        string levelJson = EditorPrefs.GetString(STATE_KEY + "CurrentLevel", "");
        if (!string.IsNullOrEmpty(levelJson))
        {
            CurrentLevel = JsonUtility.FromJson<LevelData>(levelJson);
        }

        string gridJson = EditorPrefs.GetString(STATE_KEY + "GridSettings", "");
        if (!string.IsNullOrEmpty(gridJson))
        {
            GridSettings = JsonUtility.FromJson<GridSettings>(gridJson);
        }

        string previewJson = EditorPrefs.GetString(STATE_KEY + "PreviewSettings", "");
        if (!string.IsNullOrEmpty(previewJson))
        {
            PreviewSettings = JsonUtility.FromJson<PreviewSettings>(previewJson);
        }
    }

    public void ClearState()
    {
        EditorPrefs.DeleteKey(STATE_KEY + "CurrentLevel");
        EditorPrefs.DeleteKey(STATE_KEY + "GridSettings");
        EditorPrefs.DeleteKey(STATE_KEY + "PreviewSettings");
        SelectedTiles.Clear();
    }
}

[System.Serializable]
public class GridSettings
{
    public int Width = 10;
    public int Height = 10;
    public float CellSize = 1.0f;
    public bool ShowGrid = true;
    public bool SnapToGrid = true;
}

[System.Serializable]
public class PreviewSettings
{
    public bool ShowPreview = true;
    public bool ShowColliders = true;
    public bool ShowTileIds = true;
    public float PreviewScale = 1.0f;
}
