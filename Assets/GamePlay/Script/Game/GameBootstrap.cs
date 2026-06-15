using UnityEngine;

namespace Corrnect.Game
{
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureGameControllerExists()
        {
            if (Object.FindFirstObjectByType<GameController>() != null)
                return;

            var gameObject = new GameObject("GameController");
            gameObject.AddComponent<GameController>();
        }
    }
}
