using System.Collections.Generic;
using System.Linq;
using Corrnect.Grid;
using Corrnect.Swarm;
using UnityEngine;

namespace Corrnect.Systems
{
    public static class MergeSystem
    {
        public static void MergeGroups(List<SwarmGroup> groups, GridManager grid)
        {
            if (groups.Count < 2)
                return;

            var mergeClusters = FindMergeClusters(groups, grid);

            foreach (var cluster in mergeClusters)
            {
                if (cluster.Count < 2)
                    continue;

                var primary = cluster[0];

                for (var i = 1; i < cluster.Count; i++)
                {
                    var absorbed = cluster[i];
                    primary.Absorb(absorbed, grid);
                    groups.Remove(absorbed);
                    Object.Destroy(absorbed.gameObject);
                }

                Debug.Log($"Merged swarm. Nodes: {primary.NodeCount}");
            }
        }

        private static List<List<SwarmGroup>> FindMergeClusters(List<SwarmGroup> groups, GridManager grid)
        {
            var parent = Enumerable.Range(0, groups.Count).ToArray();

            int Find(int index)
            {
                if (parent[index] != index)
                    parent[index] = Find(parent[index]);
                return parent[index];
            }

            void Union(int a, int b)
            {
                var rootA = Find(a);
                var rootB = Find(b);
                if (rootA != rootB)
                    parent[rootB] = rootA;
            }

            var occupiedCells = groups
                .Select(group => group.GetOccupiedGridCells(grid))
                .ToList();

            for (var i = 0; i < groups.Count; i++)
            {
                for (var j = i + 1; j < groups.Count; j++)
                {
                    if (ShareOrTouchAnyCell(occupiedCells[i], occupiedCells[j]))
                        Union(i, j);
                }
            }

            var clusters = new Dictionary<int, List<SwarmGroup>>();

            for (var i = 0; i < groups.Count; i++)
            {
                var root = Find(i);
                if (!clusters.ContainsKey(root))
                    clusters[root] = new List<SwarmGroup>();

                clusters[root].Add(groups[i]);
            }

            return clusters.Values.ToList();
        }

        private static bool ShareOrTouchAnyCell(HashSet<Vector2Int> a, HashSet<Vector2Int> b)
        {
            foreach (var cell in a)
            {
                if (b.Contains(cell))
                    return true;

                if (b.Contains(cell + Vector2Int.up)
                    || b.Contains(cell + Vector2Int.down)
                    || b.Contains(cell + Vector2Int.left)
                    || b.Contains(cell + Vector2Int.right))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
