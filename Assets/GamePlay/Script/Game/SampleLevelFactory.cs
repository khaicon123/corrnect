using System.Collections.Generic;
using Corrnect.Core;
using Corrnect.Grid;
using UnityEngine;

namespace Corrnect.Game
{
    public static class SampleLevelFactory
    {
        public static LevelDefinition CreateTutorialLevel()
        {
            var level = ScriptableObject.CreateInstance<LevelDefinition>();
            level.name = "TutorialLevel";
            level.Configure(
                7,
                7,
                new[]
                {
                    ".......",
                    "..###..",
                    "..#.#..",
                    "..###..",
                    ".......",
                    ".......",
                    "......."
                },
                new List<UnitSpawnData>
                {
                    new() { position = new Vector2Int(1, 1), unitType = UnitType.Horizontal },
                    new() { position = new Vector2Int(5, 1), unitType = UnitType.Vertical },
                    new() { position = new Vector2Int(1, 5), unitType = UnitType.Vertical },
                    new() { position = new Vector2Int(5, 5), unitType = UnitType.Horizontal }
                });

            return level;
        }
    }
}
