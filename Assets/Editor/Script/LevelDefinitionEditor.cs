#if UNITY_EDITOR
using Corrnect.Core;
using Corrnect.Grid;
using UnityEditor;
using UnityEngine;

namespace Corrnect.Editor
{
    public static class LevelDefinitionMenu
    {
        [MenuItem("Corrnect/Create Tutorial Level Asset")]
        public static void CreateTutorialLevelAsset()
        {
            var level = ScriptableObject.CreateInstance<LevelDefinition>();
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
                new System.Collections.Generic.List<UnitSpawnData>
                {
                    new() { position = new Vector2Int(1, 1), unitType = UnitType.Horizontal },
                    new() { position = new Vector2Int(5, 1), unitType = UnitType.Vertical },
                    new() { position = new Vector2Int(1, 5), unitType = UnitType.Vertical },
                    new() { position = new Vector2Int(5, 5), unitType = UnitType.Horizontal }
                });

            var path = AssetDatabase.GenerateUniqueAssetPath("Assets/GamePlay/Level01.asset");
            AssetDatabase.CreateAsset(level, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = level;
        }
    }
}
#endif
