#if UNITY_EDITOR
using Corrnect.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Corrnect.Editor
{
    public static class SceneSetupMenu
    {
        [MenuItem("Corrnect/Setup Game In Current Scene")]
        public static void SetupGameInCurrentScene()
        {
            if (Object.FindFirstObjectByType<GameController>() != null)
            {
                Debug.Log("GameController already exists in scene.");
                return;
            }

            var gameObject = new GameObject("GameController");
            gameObject.AddComponent<GameController>();

            EditorSceneManager.MarkSceneDirty(gameObject.scene);
            Selection.activeGameObject = gameObject;
            Debug.Log("GameController added. Press Play and use Arrow keys / WASD.");
        }
    }
}
#endif
