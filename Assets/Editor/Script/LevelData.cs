using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Core data structure for a puzzle level
/// </summary>
[System.Serializable]
public class LevelData
{
    public string LevelName;
    public int LevelId;
    public Vector2Int GridSize;
    public List<TileData> Tiles;
    public LevelProperties Properties;
    public DateTime LastModified;

    public LevelData()
    {
        LevelName = "New Level";
        LevelId = 0;
        GridSize = new Vector2Int(10, 10);
        Tiles = new List<TileData>();
        Properties = new LevelProperties();
        LastModified = DateTime.Now;
    }

    public LevelData(string name, int id, Vector2Int size)
    {
        LevelName = name;
        LevelId = id;
        GridSize = size;
        Tiles = new List<TileData>();
        Properties = new LevelProperties();
        LastModified = DateTime.Now;

        // Initialize grid with empty tiles
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                Tiles.Add(new TileData(x, y));
            }
        }
    }

    public TileData GetTile(int x, int y)
    {
        if (x < 0 || x >= GridSize.x || y < 0 || y >= GridSize.y)
            return null;

        int index = y * GridSize.x + x;
        if (index < 0 || index >= Tiles.Count)
            return null;

        return Tiles[index];
    }

    public void SetTile(int x, int y, TileData tile)
    {
        if (x < 0 || x >= GridSize.x || y < 0 || y >= GridSize.y)
            return;

        int index = y * GridSize.x + x;
        if (index >= 0 && index < Tiles.Count)
        {
            Tiles[index] = tile;
            LastModified = DateTime.Now;
        }
    }
}

[System.Serializable]
public class TileData
{
    public int X;
    public int Y;
    public int TileTypeId;
    public string TileName;
    public bool IsWalkable;
    public bool HasCollider;
    public Dictionary<string, string> CustomProperties;

    public TileData()
    {
        TileTypeId = 0;
        TileName = "Empty";
        IsWalkable = true;
        HasCollider = false;
        CustomProperties = new Dictionary<string, string>();
    }

    public TileData(int x, int y)
    {
        X = x;
        Y = y;
        TileTypeId = 0;
        TileName = "Empty";
        IsWalkable = true;
        HasCollider = false;
        CustomProperties = new Dictionary<string, string>();
    }
}

[System.Serializable]
public class LevelProperties
{
    public string Difficulty = "Normal";
    public int TargetScore = 1000;
    public int TimeLimit = 300;
    public string[] RequiredItems = new string[0];
    public string Description = "";

    public LevelProperties()
    {
    }
}
