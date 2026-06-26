using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Handles grid editing and visualization
/// </summary>
public class GridEditor
{
    private LevelEditorState editorState;
    private Vector2 gridScrollPos;
    private int selectedGridX = -1;
    private int selectedGridY = -1;
    private bool multiSelectMode = false;
    private HashSet<Vector2Int> selectedCellPositions = new HashSet<Vector2Int>();
    private bool isDragging = false;
    private TileType dragPalette = null;
    private bool dragPainted = false;

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

        EditorGUILayout.HelpBox("Chọn palette ở tab Palette, sau đó click vào ô để đổi tile. Bật Multi-Select Mode để chọn nhiều ô cùng lúc.", MessageType.Info);

        TileType selectedPalette = editorState.SelectedTileType;
        string selectedPaletteLabel = selectedPalette != null ? selectedPalette.Name : "None";
        EditorGUILayout.LabelField("Selected Palette", selectedPaletteLabel);

        int newWidth = EditorGUILayout.IntField("Width", editorState.GridSettings.Width);
        int newHeight = EditorGUILayout.IntField("Height", editorState.GridSettings.Height);
        float oldCellSize = editorState.GridSettings.CellSize;
        editorState.GridSettings.Width = Mathf.Max(1, newWidth);
        editorState.GridSettings.Height = Mathf.Max(1, newHeight);
        editorState.GridSettings.CellSize = Mathf.Max(0.1f, EditorGUILayout.FloatField("Cell Size", editorState.GridSettings.CellSize));
        editorState.GridSettings.ShowGrid = EditorGUILayout.Toggle("Show Grid", editorState.GridSettings.ShowGrid);
        editorState.GridSettings.SnapToGrid = EditorGUILayout.Toggle("Snap to Grid", editorState.GridSettings.SnapToGrid);

        bool cellSizeChanged = !Mathf.Approximately(oldCellSize, editorState.GridSettings.CellSize);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Apply Grid Size"))
        {
            ApplyGridSize(editorState.GridSettings.Width, editorState.GridSettings.Height);
        }

        if (GUILayout.Button("Rebuild Scene Grid"))
        {
            CreateGridInScene();
        }
        EditorGUILayout.EndHorizontal();

        if (cellSizeChanged && editorState.CurrentLevel != null)
        {
            CreateGridInScene();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset All Tiles to Floor", GUILayout.Height(24)))
        {
            ResetAllTilesToFloor();
        }
        if (GUILayout.Button("Clear All Tiles", GUILayout.Height(24)))
        {
            ClearAllTiles();
        }
        if (GUILayout.Button("Remove Scene Grid", GUILayout.Height(24)))
        {
            RemoveSceneGrid();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        multiSelectMode = GUILayout.Toggle(multiSelectMode, "Multi-Select Mode", "Button", GUILayout.Height(24));
        if (GUILayout.Button("Clear Selection", GUILayout.Height(24)))
        {
            selectedCellPositions.Clear();
            selectedGridX = -1;
            selectedGridY = -1;
        }
        EditorGUILayout.EndHorizontal();

        editorState.SaveState();
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

        HandleDragPaint();
    }

    private void DrawTileButton(TileData tile, int x, int y)
    {
        bool isSelected = selectedGridX == x && selectedGridY == y;
        bool isMultiSelected = selectedCellPositions.Contains(new Vector2Int(x, y));
        GUI.backgroundColor = isMultiSelected ? Color.cyan : (isSelected ? Color.yellow : (tile.IsWalkable ? Color.white : Color.gray));

        string buttonLabel = $"{tile.TileName}\n({x},{y})";
        Rect tileRect = GUILayoutUtility.GetRect(60, 60);
        bool buttonClicked = GUI.Button(tileRect, buttonLabel);

        Event currentEvent = Event.current;
        TileType selectedPalette = editorState.SelectedTileType;

        if (buttonClicked)
        {
            if (multiSelectMode)
            {
                Vector2Int cellPos = new Vector2Int(x, y);
                if (selectedCellPositions.Contains(cellPos))
                    selectedCellPositions.Remove(cellPos);
                else
                    selectedCellPositions.Add(cellPos);
            }
            else
            {
                selectedGridX = x;
                selectedGridY = y;
                selectedCellPositions.Clear();
                selectedCellPositions.Add(new Vector2Int(x, y));
            }

            editorState.SelectedTiles.Clear();
            editorState.SelectedTiles.Add(tile);

            if (selectedPalette != null)
            {
                StartDrag(selectedPalette);
                PaintTileAt(x, y);
            }
        }
        else if (currentEvent.type == EventType.MouseDrag && isDragging && tileRect.Contains(currentEvent.mousePosition))
        {
            PaintTileAt(x, y);
            currentEvent.Use();
        }

        if (currentEvent.type == EventType.MouseUp && isDragging)
        {
            StopDrag();
            currentEvent.Use();
        }

        GUI.backgroundColor = Color.white;
    }

    private void StartDrag(TileType palette)
    {
        dragPalette = palette;
        isDragging = true;
        dragPainted = false;
    }

    private void StopDrag()
    {
        if (isDragging)
        {
            isDragging = false;
            dragPalette = null;
            dragPainted = false;
            editorState.SaveState();
            CreateGridInScene();
        }
    }

    private void PaintTileAt(int x, int y)
    {
        if (editorState.CurrentLevel == null || dragPalette == null)
            return;

        TileData tile = editorState.CurrentLevel.GetTile(x, y);
        if (tile == null)
            return;

        ApplyTileType(tile, dragPalette);
        dragPainted = true;
        editorState.SaveState();
        CreateGridInScene();
    }

    private void HandleDragPaint()
    {
        Event currentEvent = Event.current;
        if (currentEvent == null)
            return;

        if (currentEvent.type == EventType.MouseUp && isDragging)
        {
            StopDrag();
        }
    }

    private static Sprite tileSprite;

    private static Sprite GetTileSprite()
    {
        if (tileSprite == null)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            tileSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        }

        return tileSprite;
    }

    private void ResetAllTilesToFloor()
    {
        if (editorState.CurrentLevel == null)
            return;

        for (int y = 0; y < editorState.CurrentLevel.GridSize.y; y++)
        {
            for (int x = 0; x < editorState.CurrentLevel.GridSize.x; x++)
            {
                TileData tile = editorState.CurrentLevel.GetTile(x, y);
                if (tile == null)
                    continue;

                tile.TileTypeId = 1;
                tile.TileName = "Floor";
                tile.IsWalkable = true;
                tile.HasCollider = false;
                tile.TileColor = Color.yellow;
            }
        }

        editorState.SaveState();
        CreateGridInScene();
    }

    private void ClearAllTiles()
    {
        if (editorState.CurrentLevel == null)
            return;

        for (int y = 0; y < editorState.CurrentLevel.GridSize.y; y++)
        {
            for (int x = 0; x < editorState.CurrentLevel.GridSize.x; x++)
            {
                TileData tile = editorState.CurrentLevel.GetTile(x, y);
                if (tile == null)
                    continue;

                tile.TileTypeId = 0;
                tile.TileName = "Empty";
                tile.IsWalkable = true;
                tile.HasCollider = false;
                tile.TileColor = Color.white;
            }
        }

        editorState.SaveState();
        CreateGridInScene();
    }

    private void RemoveSceneGrid()
    {
        if (editorState.CurrentLevel == null)
            return;

        string gridName = $"Grid_{editorState.CurrentLevel.LevelName}";
        GameObject existingGrid = GameObject.Find(gridName);
        if (existingGrid != null)
        {
            Undo.DestroyObjectImmediate(existingGrid);
        }
    }

    public void CreateGridInScene()
    {
        if (editorState.CurrentLevel == null)
        {
            EditorUtility.DisplayDialog("Error", "No level loaded", "OK");
            return;
        }

        string gridName = $"Grid_{editorState.CurrentLevel.LevelName}";
        GameObject existingGrid = GameObject.Find(gridName);
        if (existingGrid != null)
        {
            Undo.DestroyObjectImmediate(existingGrid);
        }

        GameObject gridParent = new GameObject(gridName);
        Undo.RegisterCreatedObjectUndo(gridParent, "Create Grid");

        for (int y = 0; y < editorState.CurrentLevel.GridSize.y; y++)
        {
            for (int x = 0; x < editorState.CurrentLevel.GridSize.x; x++)
            {
                TileData tile = editorState.CurrentLevel.GetTile(x, y);
                GameObject tileObj = new GameObject($"Tile_{x}_{y}");
                tileObj.transform.SetParent(gridParent.transform);
                tileObj.transform.position = new Vector3(x * editorState.GridSettings.CellSize, y * editorState.GridSettings.CellSize, 0);
                tileObj.transform.localScale = new Vector3(editorState.GridSettings.CellSize, editorState.GridSettings.CellSize, 1);

                var spriteRenderer = tileObj.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = GetTileSprite();
                spriteRenderer.color = tile.TileColor;
                spriteRenderer.sortingOrder = 0;

                if (tile.HasCollider)
                {
                    var collider = tileObj.AddComponent<BoxCollider2D>();
                    collider.size = Vector2.one;
                }
            }
        }
    }

    private void ApplyGridSize(int width, int height)
    {
        if (editorState.CurrentLevel == null)
        {
            EditorUtility.DisplayDialog("Error", "No level loaded", "OK");
            return;
        }

        width = Mathf.Max(1, width);
        height = Mathf.Max(1, height);

        if (editorState.CurrentLevel.GridSize.x == width && editorState.CurrentLevel.GridSize.y == height)
        {
            CreateGridInScene();
            return;
        }

        Vector2Int oldSize = editorState.CurrentLevel.GridSize;
        var newTiles = new System.Collections.Generic.List<TileData>(width * height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                TileData oldTile = null;
                if (x < oldSize.x && y < oldSize.y)
                {
                    oldTile = editorState.CurrentLevel.GetTile(x, y);
                }

                if (oldTile != null)
                {
                    newTiles.Add(new TileData(x, y)
                    {
                        TileTypeId = oldTile.TileTypeId,
                        TileName = oldTile.TileName,
                        IsWalkable = oldTile.IsWalkable,
                        HasCollider = oldTile.HasCollider,
                        TileColor = oldTile.TileColor,
                        CustomProperties = oldTile.CustomProperties != null ? new System.Collections.Generic.Dictionary<string, string>(oldTile.CustomProperties) : new System.Collections.Generic.Dictionary<string, string>()
                    });
                }
                else
                {
                    newTiles.Add(new TileData(x, y, true));
                }
            }
        }

        editorState.CurrentLevel.GridSize = new Vector2Int(width, height);
        editorState.CurrentLevel.Tiles = newTiles;
        editorState.SaveState();
        CreateGridInScene();
    }

    private void ApplyTileType(TileData tile, TileType paletteTile)
    {
        if (tile == null || paletteTile == null)
            return;

        tile.TileTypeId = paletteTile.Id;
        tile.TileName = paletteTile.Name;
        tile.IsWalkable = paletteTile.IsWalkable;
        tile.HasCollider = paletteTile.HasCollider;
        tile.TileColor = paletteTile.Color;
    }

    private void ApplyPaletteToSelection(TileType paletteTile)
    {
        if (paletteTile == null || editorState.CurrentLevel == null)
            return;

        foreach (var cellPos in selectedCellPositions)
        {
            TileData tile = editorState.CurrentLevel.GetTile(cellPos.x, cellPos.y);
            if (tile == null)
                continue;

            ApplyTileType(tile, paletteTile);
        }

        editorState.SaveState();
        CreateGridInScene();
    }
}
