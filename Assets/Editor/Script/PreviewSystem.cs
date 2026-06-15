using UnityEngine;
using UnityEditor;

/// <summary>
/// Previews the level in the editor
/// </summary>
public class PreviewSystem
{
    private LevelEditorState editorState;
    private Texture2D previewTexture;

    public PreviewSystem(LevelEditorState state)
    {
        editorState = state;
    }

    public void DrawGUI()
    {
        EditorGUILayout.LabelField("Level Preview", EditorStyles.boldLabel);

        DrawPreviewSettings();
        EditorGUILayout.Space();
        DrawPreview();
    }

    private void DrawPreviewSettings()
    {
        EditorGUILayout.LabelField("Preview Options", EditorStyles.label);

        EditorGUI.BeginChangeCheck();

        editorState.PreviewSettings.ShowPreview = EditorGUILayout.Toggle("Show Preview", editorState.PreviewSettings.ShowPreview);
        editorState.PreviewSettings.ShowColliders = EditorGUILayout.Toggle("Show Colliders", editorState.PreviewSettings.ShowColliders);
        editorState.PreviewSettings.ShowTileIds = EditorGUILayout.Toggle("Show Tile IDs", editorState.PreviewSettings.ShowTileIds);
        editorState.PreviewSettings.PreviewScale = EditorGUILayout.FloatField("Preview Scale", editorState.PreviewSettings.PreviewScale);

        if (EditorGUI.EndChangeCheck())
        {
            GeneratePreview();
        }
    }

    private void DrawPreview()
    {
        if (!editorState.PreviewSettings.ShowPreview)
        {
            EditorGUILayout.HelpBox("Preview disabled", MessageType.Info);
            return;
        }

        if (editorState.CurrentLevel == null)
        {
            EditorGUILayout.HelpBox("No level loaded", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField("Grid Preview", EditorStyles.label);

        // Draw grid preview
        float cellSize = 20 * editorState.PreviewSettings.PreviewScale;
        int gridWidth = editorState.CurrentLevel.GridSize.x;
        int gridHeight = editorState.CurrentLevel.GridSize.y;

        float previewWidth = gridWidth * cellSize;
        float previewHeight = gridHeight * cellSize;

        Rect previewRect = GUILayoutUtility.GetRect(previewWidth, previewHeight);

        GUI.Box(previewRect, "");

        // Draw grid cells
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                TileData tile = editorState.CurrentLevel.GetTile(x, y);
                if (tile == null)
                    continue;

                Rect cellRect = new Rect(
                    previewRect.x + x * cellSize,
                    previewRect.y + (gridHeight - y - 1) * cellSize,
                    cellSize,
                    cellSize
                );

                // Draw cell background
                Color cellColor = tile.IsWalkable ? new Color(1, 1, 1, 0.5f) : new Color(0.3f, 0.3f, 0.3f, 0.5f);
                if (tile.HasCollider)
                    cellColor = Color.red * 0.5f;

                EditorGUI.DrawRect(cellRect, cellColor);

                // Draw grid lines
                GUI.color = Color.gray;
                GUI.Box(cellRect, "");
                GUI.color = Color.white;

                // Draw tile info
                if (editorState.PreviewSettings.ShowTileIds)
                {
                    GUI.Label(cellRect, tile.TileName, new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 8,
                        alignment = TextAnchor.MiddleCenter
                    });
                }
            }
        }

        // Draw legend
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Legend", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(15, 15), Color.white);
        EditorGUILayout.LabelField("Walkable", GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(15, 15), new Color(0.3f, 0.3f, 0.3f));
        EditorGUILayout.LabelField("Non-Walkable", GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUI.DrawRect(GUILayoutUtility.GetRect(15, 15), Color.red * 0.7f);
        EditorGUILayout.LabelField("Collider", GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();
    }

    private void GeneratePreview()
    {
        if (editorState.CurrentLevel == null)
            return;

        int gridWidth = editorState.CurrentLevel.GridSize.x;
        int gridHeight = editorState.CurrentLevel.GridSize.y;
        int cellSize = (int)(20 * editorState.PreviewSettings.PreviewScale);

        previewTexture = new Texture2D(gridWidth * cellSize, gridHeight * cellSize, TextureFormat.RGB24, false);

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                TileData tile = editorState.CurrentLevel.GetTile(x, y);
                if (tile == null)
                    continue;

                Color tileColor = tile.IsWalkable ? Color.white : Color.gray;
                if (tile.HasCollider)
                    tileColor = Color.red;

                // Fill cell with color
                for (int py = 0; py < cellSize; py++)
                {
                    for (int px = 0; px < cellSize; px++)
                    {
                        previewTexture.SetPixel(x * cellSize + px, y * cellSize + py, tileColor);
                    }
                }
            }
        }

        previewTexture.Apply();
    }
}
