using System;
using System.Collections.Generic;
using Corrnect.Core;
using UnityEngine;

namespace Corrnect.Grid
{
    [CreateAssetMenu(fileName = "LevelDefinition", menuName = "Corrnect/Level Definition")]
    public class LevelDefinition : ScriptableObject
    {
        [SerializeField] private int width = 5;
        [SerializeField] private int height = 5;
        [SerializeField] private string[] rows =
        {
            "..#..",
            ".....",
            "..#..",
            ".....",
            "....."
        };
        public GridManager gridManager;

        [SerializeField] private List<UnitSpawnData> unitSpawns = new();

        public int Width => width;
        public int Height => height;
        public IReadOnlyList<UnitSpawnData> UnitSpawns => unitSpawns;

        public CellType GetCell(int x, int y)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
                return CellType.Wall;

            if (rows == null || y >= rows.Length || string.IsNullOrEmpty(rows[y]))
                return CellType.Floor;

            var row = rows[y];
            if (x >= row.Length)
                return CellType.Floor;

            return row[x] == '#' ? CellType.Wall : CellType.Floor;
        }

        public void Configure(int mapWidth, int mapHeight, string[] mapRows, GridManager gridMgr, List<UnitSpawnData> spawns)
        {
            width = mapWidth;
            height = mapHeight;
            rows = mapRows;
            unitSpawns = spawns;
            gridManager = gridMgr;
        }
          public void Configure(int mapWidth, int mapHeight, string[] mapRows, List<UnitSpawnData> spawns)
        {
            width = mapWidth;
            height = mapHeight;
            rows = mapRows;
            unitSpawns = spawns;
            
        }

    }

    [Serializable]
    public struct UnitSpawnData
    {
        public Vector2Int position;
        public UnitType unitType;
    }
}
