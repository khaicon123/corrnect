using UnityEngine;
using UnityEditor;

/// <summary>
/// Handles grid editing and visualization
/// </summary>
public class GridEditor
{
    private LevelEditorState editorState;
    private Vector2 gridScrollPos;
    private int selectedGridX = -1;
    private int selectedGridY = -1;

    public GridEditor(LevelEditorState state)
    {
        editorState = state;
    }

    public void DrawGUI()
    {
        if (editorState.CurrentLevel == null)
        {
            EditorGUILayout.HelpBox("No level loaded. Create or load a level first.", MessageType.Info);
            return;
        }

        DrawGridSettings();
        EditorGUILayout.Space();
        DrawGridEditor();
    }

    private void DrawGridSettings()
    {
        EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        editorState.GridSettings.Width = EditorGUILayout.IntField("Width", editorState.GridSettings.Width);
        editorState.GridSettings.Height = EditorGUILayout.IntField("Height", editorState.GridSettings.Height);
        editorState.GridSettings.CellSize = EditorGUILayout.FloatField("Cell Size", editorState.GridSettings.CellSize);
        editorState.GridSettings.ShowGrid = EditorGUILayout.Toggle("Show Grid", editorState.GridSettings.ShowGrid);
        editorState.GridSettings.SnapToGrid = EditorGUILayout.Toggle("Snap to Grid", editorState.GridSettings.SnapToGrid);

        if (EditorGUI.EndChangeCheck())
        {
            editorState.SaveState();
        }
    }

    private void DrawGridEditor()
    {
        EditorGUILayout.LabelField("Level Grid", EditorStyles.boldLabel);

        gridScrollPos = EditorGUILayout.BeginScrollView(gridScrollPos, GUILayout.ExpandHeight(true));

        int gridWidth = editorState.CurrentLevel.GridSize.x;
        int gridHeight = editorState.CurrentLevel.GridSize.y;

        for (int y = gridHeight - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();

            for (int x = 0; x < gridWidth; x++)
            {
                TileData tile = editorState.CurrentLevel.GetTile(x, y);
                DrawTileButton(tile, x, y);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawTileButton(TileData tile, int x, int y)
    {
        bool isSelected = selectedGridX == x && selectedGridY == y;
        GUI.backgroundColor = isSelected ? Color.yellow : (tile.IsWalkable ? Color.white : Color.gray);

        string buttonLabel = $"{tile.TileName}\n({x},{y})";
        
        if (GUILayout.Button(buttonLabel, GUILayout.Width(60), GUILayout.Height(60)))
        {
            selectedGridX = x;
            selectedGridY = y;
            editorState.SelectedTiles.Clear();
            editorState.SelectedTiles.Add(tile);
        }

        GUI.backgroundColor = Color.white;
    }

    public void CreateGridInScene()
    {
        if (editorState.CurrentLevel == null)
        {
            EditorUtility.DisplayDialog("Error", "No level loaded", "OK");
            return;
        }

        GameObject gridParent = new GameObject($"Grid_{editorState.CurrentLevel.LevelName}");
        
        for (int y = 0; y < editorState.CurrentLevel.GridSize.y; y++)
        {
            for (int x = 0; x < editorState.CurrentLevel.GridSize.x; x++)
            {
                TileData tile = editorState.CurrentLevel.GetTile(x, y);
                GameObject tileObj = new GameObject($"Tile_{x}_{y}");
                tileObj.transform.parent = gridParent.transform;
                tileObj.transform.position = new Vector3(x * editorState.GridSettings.CellSize, y * editorState.GridSettings.CellSize, 0);

                if (tile.HasCollider)
                {
                    tileObj.AddComponent<BoxCollider2D>();
                }
            }
        }

        Undo.RegisterCreatedObjectUndo(gridParent, "Create Grid");
    }
}
