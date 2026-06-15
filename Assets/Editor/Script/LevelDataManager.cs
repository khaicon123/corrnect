using UnityEngine;
using UnityEditor;

/// <summary>
/// Manages level data and properties
/// </summary>
public class LevelDataManager
{
    private LevelEditorState editorState;

    public LevelDataManager(LevelEditorState state)
    {
        editorState = state;
    }

    public void CreateNewLevel()
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("10x10"), false, () => CreateLevel("New Level", 10, 10));
        menu.AddItem(new GUIContent("15x15"), false, () => CreateLevel("New Level", 15, 15));
        menu.AddItem(new GUIContent("20x20"), false, () => CreateLevel("New Level", 20, 20));
        menu.ShowAsContext();
    }

    private void CreateLevel(string name, int width, int height)
    {
        editorState.CurrentLevel = new LevelData(name, 1, new Vector2Int(width, height));
        editorState.SaveState();
        EditorUtility.DisplayDialog("Success", $"Created new level: {name}", "OK");
    }

    public void DrawPropertiesGUI()
    {
        if (editorState.CurrentLevel == null)
        {
            EditorGUILayout.HelpBox("No level loaded. Create or load a level first.", MessageType.Info);
            return;
        }

        DrawLevelInfo();
        EditorGUILayout.Space();
        DrawLevelProperties();
        EditorGUILayout.Space();
        DrawGridStatistics();
    }

    private void DrawLevelInfo()
    {
        EditorGUILayout.LabelField("Level Information", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        editorState.CurrentLevel.LevelName = EditorGUILayout.TextField("Level Name", editorState.CurrentLevel.LevelName);
        editorState.CurrentLevel.LevelId = EditorGUILayout.IntField("Level ID", editorState.CurrentLevel.LevelId);

        EditorGUILayout.Vector2IntField("Grid Size", editorState.CurrentLevel.GridSize);
        EditorGUILayout.LabelField("Last Modified", editorState.CurrentLevel.LastModified.ToString());

        if (EditorGUI.EndChangeCheck())
        {
            editorState.SaveState();
        }
    }

    private void DrawLevelProperties()
    {
        EditorGUILayout.LabelField("Level Properties", EditorStyles.boldLabel);

        LevelProperties props = editorState.CurrentLevel.Properties;

        EditorGUI.BeginChangeCheck();

        props.Difficulty = EditorGUILayout.TextField("Difficulty", props.Difficulty);
        props.TargetScore = EditorGUILayout.IntField("Target Score", props.TargetScore);
        props.TimeLimit = EditorGUILayout.IntField("Time Limit (sec)", props.TimeLimit);
        props.Description = EditorGUILayout.TextArea(props.Description, GUILayout.Height(80));

        EditorGUILayout.LabelField("Required Items", EditorStyles.label);
        if (GUILayout.Button("Edit Required Items", GUILayout.Height(20)))
        {
            // You can implement a more complex editor for arrays here
        }

        if (EditorGUI.EndChangeCheck())
        {
            editorState.SaveState();
        }
    }

    private void DrawGridStatistics()
    {
        EditorGUILayout.LabelField("Grid Statistics", EditorStyles.boldLabel);

        int totalTiles = editorState.CurrentLevel.Tiles.Count;
        int walkableTiles = 0;
        int collidableTiles = 0;

        foreach (var tile in editorState.CurrentLevel.Tiles)
        {
            if (tile.IsWalkable)
                walkableTiles++;
            if (tile.HasCollider)
                collidableTiles++;
        }

        EditorGUILayout.LabelField($"Total Tiles: {totalTiles}");
        EditorGUILayout.LabelField($"Walkable Tiles: {walkableTiles}");
        EditorGUILayout.LabelField($"Tiles with Colliders: {collidableTiles}");
    }
}
