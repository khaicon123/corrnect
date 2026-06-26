using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Manages tile palette and tile selection
/// </summary>
public class TilePaletteEditor
{
    private LevelEditorState editorState;
    private List<TileType> tileTypes;
    private int selectedTileTypeId = -1;
    private Vector2 paletteScrollPos;

    public TilePaletteEditor(LevelEditorState state)
    {
        editorState = state;
        LoadTilePalette();

        if (selectedTileTypeId < 0 && tileTypes.Count > 0)
        {
            selectedTileTypeId = tileTypes.Find(t => t.Name == "Floor")?.Id ?? tileTypes[0].Id;
            editorState.SelectedTileType = tileTypes.Find(t => t.Id == selectedTileTypeId);
        }
    }

    public void DrawGUI()
    {
        EditorGUILayout.LabelField("Tile Palette", EditorStyles.boldLabel);

        DrawTileTypeSelection();
        EditorGUILayout.Space();
        DrawTileTypeProperties();
    }

    private void DrawTileTypeSelection()
    {
        EditorGUILayout.LabelField("Available Tiles", EditorStyles.label);

        paletteScrollPos = EditorGUILayout.BeginScrollView(paletteScrollPos, GUILayout.ExpandHeight(true));

        if (tileTypes == null || tileTypes.Count == 0)
        {
            EditorGUILayout.HelpBox("No tiles in palette. Add tiles to palette first.", MessageType.Info);
        }
        else
        {
            for (int i = 0; i < tileTypes.Count; i++)
            {
                DrawTileTypeButton(tileTypes[i], i);
            }
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        if (GUILayout.Button("+ Add New Tile Type", GUILayout.Height(30)))
        {
            AddNewTileType();
        }
    }

    private void DrawTileTypeButton(TileType tileType, int index)
    {
        bool isSelected = selectedTileTypeId == tileType.Id;
        GUI.backgroundColor = isSelected ? Color.cyan : Color.white;

        string buttonLabel = $"{tileType.Name}\nID: {tileType.Id}";
        
        if (GUILayout.Button(buttonLabel, GUILayout.Height(50)))
        {
            selectedTileTypeId = tileType.Id;
            editorState.SelectedTileType = tileType;
        }

        GUI.backgroundColor = Color.white;
    }

    private void DrawTileTypeProperties()
    {
        if (selectedTileTypeId < 0 || tileTypes == null || tileTypes.Count == 0)
        {
            EditorGUILayout.HelpBox("Select a tile type to view/edit properties", MessageType.Info);
            return;
        }

        TileType selectedTile = tileTypes.Find(t => t.Id == selectedTileTypeId);
        if (selectedTile == null)
            return;

        EditorGUILayout.LabelField("Tile Properties", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        selectedTile.Name = EditorGUILayout.TextField("Name", selectedTile.Name);
        selectedTile.IsWalkable = EditorGUILayout.Toggle("Walkable", selectedTile.IsWalkable);
        selectedTile.HasCollider = EditorGUILayout.Toggle("Has Collider", selectedTile.HasCollider);
        selectedTile.Color = EditorGUILayout.ColorField("Color", selectedTile.Color);
        selectedTile.Icon = (Texture2D)EditorGUILayout.ObjectField("Icon", selectedTile.Icon, typeof(Texture2D), false);

        if (EditorGUI.EndChangeCheck())
        {
            // Changes are applied directly to the tile type
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Delete This Tile Type", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("Delete Tile Type", 
                $"Are you sure you want to delete '{selectedTile.Name}'?", "Delete", "Cancel"))
            {
                tileTypes.Remove(selectedTile);
                selectedTileTypeId = -1;
            }
        }
    }

    private void AddNewTileType()
    {
        if (tileTypes == null)
            tileTypes = new List<TileType>();

        int newId = tileTypes.Count > 0 ? tileTypes[tileTypes.Count - 1].Id + 1 : 0;
        TileType newTile = new TileType
        {
            Id = newId,
            Name = $"Tile_{newId}",
            IsWalkable = true,
            HasCollider = false,
            Color = Color.white
        };

        tileTypes.Add(newTile);
        selectedTileTypeId = newId;
        editorState.SelectedTileType = newTile;
    }

    private void LoadTilePalette()
    {
        // Load from resources or create default palette
        if (tileTypes == null)
        {
            tileTypes = new List<TileType>
            {
                new TileType { Id = 0, Name = "Wall", IsWalkable = false, HasCollider = true, Color = Color.gray },
                new TileType { Id = 1, Name = "Floor", IsWalkable = true, HasCollider = false, Color = Color.yellow },
                new TileType { Id = 2, Name = "Spike", IsWalkable = false, HasCollider = true, Color = Color.red }
            };
        }
    }

    public TileType GetSelectedTileType()
    {
        if (selectedTileTypeId < 0 || tileTypes == null)
            return null;

        return tileTypes.Find(t => t.Id == selectedTileTypeId);
    }
}

[System.Serializable]
public class TileType
{
    public int Id;
    public string Name;
    public bool IsWalkable;
    public bool HasCollider;
    public Color Color;
    public Texture2D Icon;
}
