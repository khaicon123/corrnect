using System.Collections.Generic;
using System.Linq;
using Corrnect.Core;
using Corrnect.Grid;
using Corrnect.Swarm;
using UnityEngine;

namespace Corrnect.Systems
{
    public static class MoveSystem
    {
        public static void ExecuteTurn(List<SwarmGroup> groups, Direction direction, GridManager grid)
        {
            if (groups.Count == 0)
                return;

            var startPositions = groups.ToDictionary(group => group, group => group.GridPosition);
            var plannedPositions = groups.ToDictionary(group => group, group => group.GridPosition);
            var movers = groups
                .Where(group => group.CanMove(direction, grid))
                .OrderBy(group => direction.LeadingEdgeSortKey(startPositions[group]))
                .ToList();

            var offset = direction.ToOffset();

            foreach (var group in movers)
            {
                var target = plannedPositions[group] + offset;

                if (!group.CanMoveToGridPosition(target, grid))
                    continue;

                plannedPositions[group] = target;
            }

            foreach (var group in groups)
                group.SetGridPosition(plannedPositions[group]);
        }
    }
}
