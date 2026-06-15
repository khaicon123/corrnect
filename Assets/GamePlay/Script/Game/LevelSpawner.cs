using System.Collections.Generic;
using Corrnect.Core;
using Corrnect.Grid;
using Corrnect.Swarm;
using UnityEngine;

namespace Corrnect.Game
{
    public class LevelSpawner : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;
        [SerializeField] private Transform swarmParent;
        [SerializeField] private SwarmGroup swarmGroupPrefab;

        private readonly List<SwarmGroup> _spawnedGroups = new();

        public IReadOnlyList<SwarmGroup> SpawnedGroups => _spawnedGroups;

        public void Configure(GridManager grid, Transform parent = null)
        {
            gridManager = grid;
            if (parent != null)
                swarmParent = parent;
            else if (swarmParent == null)
                swarmParent = transform;
        }

        public void SpawnLevel(LevelDefinition level)
        {
            ClearGroups();
            gridManager.Initialize(level);

            foreach (var spawn in level.UnitSpawns)
            {
                if (!gridManager.IsWalkable(spawn.position))
                {
                    Debug.LogWarning($"Cannot spawn unit at blocked cell {spawn.position}");
                    continue;
                }

                var group = CreateGroup(spawn.position, spawn.unitType);
                _spawnedGroups.Add(group);
            }
        }

        public List<SwarmGroup> GetActiveGroupsCopy()
        {
            _spawnedGroups.RemoveAll(group => group == null);
            return new List<SwarmGroup>(_spawnedGroups);
        }

        private SwarmGroup CreateGroup(Vector2Int position, UnitType unitType)
        {
            var group = swarmGroupPrefab != null
                ? Instantiate(swarmGroupPrefab, swarmParent)
                : CreateRuntimeGroup();

            group.name = $"{unitType}Group_{position.x}_{position.y}";
            group.Initialize(gridManager, position, unitType);
            return group;
        }

        private SwarmGroup CreateRuntimeGroup()
        {
            var groupObject = new GameObject("SwarmGroup");
            groupObject.transform.SetParent(swarmParent, false);

            var collider = groupObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one * 0.85f;

            return groupObject.AddComponent<SwarmGroup>();
        }

        private void ClearGroups()
        {
            foreach (var group in _spawnedGroups)
            {
                if (group != null)
                    Destroy(group.gameObject);
            }

            _spawnedGroups.Clear();
        }
    }
}
