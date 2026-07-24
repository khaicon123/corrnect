using Corrnect.Game;
using UnityEngine;

namespace Corrnect.UI
{
    public class WinOverlay : MonoBehaviour
    {
        [SerializeField] private GameController gameController;

        private void Awake()
        {
            if (gameController == null)
                gameController = FindFirstObjectByType<GameController>();
        }

        private void OnGUI()
        {
            if (gameController == null)
                return;

            string message = null;

            if (gameController.IsLevelComplete)
                message = "You Win!\nAll units merged into one swarm.";
            else if (gameController.IsGameOver)
                message = "Game Over!\nA danger object hit your unit.";

            if (string.IsNullOrEmpty(message))
                return;

            const int boxWidth = 320;
            const int boxHeight = 80;
            var rect = new Rect(
                (Screen.width - boxWidth) * 0.5f,
                (Screen.height - boxHeight) * 0.5f,
                boxWidth,
                boxHeight);

            GUI.Box(rect, message);
        }
    }
}
