using System.Collections.Generic;
using Corrnect.Core;
using Corrnect.Grid;
using Corrnect.Input;
using Corrnect.Systems;
using Corrnect.Swarm;
using UnityEngine;

namespace Corrnect.Game
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private LevelDefinition levelDefinition;
        [SerializeField] private GridManager gridManager;
        [SerializeField] private LevelSpawner levelSpawner;
        [SerializeField] private InputManager inputManager;

        private readonly List<SwarmGroup> _activeGroups = new();
        private bool _isLevelComplete;
        private bool _isGameOver;

        public bool IsLevelComplete => _isLevelComplete;
        public bool IsGameOver => _isGameOver;
        // Expose level and grid so other systems (camera, UI) can reference them
        public Corrnect.Grid.LevelDefinition CurrentLevelDefinition => levelDefinition;
        public Corrnect.Grid.GridManager Grid => gridManager;

        private void Awake()
        {
            EnsureReferences();

            if (levelDefinition == null)
                levelDefinition = SampleLevelFactory.CreateTutorialLevel();
        }

        private void Start()
        {
            LoadLevel();
        }

        private void OnEnable()
        {
            if (inputManager != null)
                inputManager.TurnInput += HandleTurnInput;
        }

        private void OnDisable()
        {
            if (inputManager != null)
                inputManager.TurnInput -= HandleTurnInput;
        }

        public void LoadLevel()
        {
            _isLevelComplete = false;
            _isGameOver = false;

            if (levelDefinition == null)
            {
                Debug.LogError("GameController requires a LevelDefinition.");
                return;
            }

            levelSpawner.SpawnLevel(levelDefinition);
            _activeGroups.Clear();
            _activeGroups.AddRange(levelSpawner.GetActiveGroupsCopy());
            MergeSystem.MergeGroups(_activeGroups, gridManager);

            if (HazardSystem.HasHazardCollision(_activeGroups, gridManager))
            {
                OnGameOver();
                return;
            }

            if (inputManager != null)
                inputManager.InputEnabled = true;
        }

        private void HandleTurnInput(Direction direction)
        {
            if (_isLevelComplete || _isGameOver || _activeGroups.Count == 0)
                return;

            MoveSystem.ExecuteTurn(_activeGroups, direction, gridManager);

            if (HazardSystem.HasHazardCollision(_activeGroups, gridManager))
            {
                OnGameOver();
                return;
            }

            MergeSystem.MergeGroups(_activeGroups, gridManager);

            if (WinSystem.IsLevelComplete(_activeGroups))
                OnLevelComplete();
        }

        private void OnLevelComplete()
        {
            _isLevelComplete = true;

            if (inputManager != null)
                inputManager.InputEnabled = false;

            Debug.Log("Level complete! All units merged into one swarm.");
        }

        private void OnGameOver()
        {
            _isGameOver = true;

            if (inputManager != null)
                inputManager.InputEnabled = false;

            Debug.Log("Game Over!");
        }

        private void EnsureReferences()
        {
            if (gridManager == null)
                gridManager = FindOrCreateComponent<GridManager>("Grid");

            if (levelSpawner == null)
            {
                var spawnerObject = new GameObject("LevelSpawner");
                spawnerObject.transform.SetParent(transform, false);
                levelSpawner = spawnerObject.AddComponent<LevelSpawner>();
            }

            levelSpawner.Configure(gridManager, levelSpawner.transform);

            if (GetComponent<UI.WinOverlay>() == null)
                gameObject.AddComponent<UI.WinOverlay>();

            if (inputManager == null)
            {
                var inputObject = new GameObject("InputManager");
                inputObject.transform.SetParent(transform, false);
                inputManager = inputObject.AddComponent<InputManager>();
            }

            var camera = Camera.main;
            if (camera != null)
            {
                camera.orthographic = true;
                camera.orthographicSize = 5f;
                camera.transform.position = new Vector3(0f, 0f, -10f);
                camera.backgroundColor = new Color(0.08f, 0.09f, 0.12f);
            }
        }

        private static T FindOrCreateComponent<T>(string objectName) where T : Component
        {
            var existing = Object.FindFirstObjectByType<T>();
            if (existing != null)
                return existing;

            var gameObject = new GameObject(objectName);
            return gameObject.AddComponent<T>();
        }
    }
}
