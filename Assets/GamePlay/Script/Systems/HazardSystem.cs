using System.Collections.Generic;
using System.Linq;
using Corrnect.Grid;
using Corrnect.Swarm;
using UnityEngine;

namespace Corrnect.Systems
{
    public static class HazardSystem
    {
        public static bool IsHazard(SwarmGroup group)
        {
            return group != null && group.MemberTypes.Any(type => type == Core.UnitType.DangerMoving || type == Core.UnitType.DangerStatic);
        }

        public static bool HasHazardCollision(List<SwarmGroup> groups, GridManager grid)
        {
            if (groups == null || groups.Count == 0 || grid == null)
                return false;

            var hazardGroups = groups.Where(IsHazard).ToList();
            if (hazardGroups.Count == 0)
                return false;

            var nonHazardGroups = groups.Where(group => !IsHazard(group)).ToList();
            if (nonHazardGroups.Count == 0)
                return false;

            var hazardCells = new HashSet<Vector2Int>();
            foreach (var hazard in hazardGroups)
                hazardCells.UnionWith(hazard.GetOccupiedGridCells(grid));

            foreach (var unit in nonHazardGroups)
            {
                var occupied = unit.GetOccupiedGridCells(grid);
                if (occupied.Overlaps(hazardCells))
                    return true;
            }

            return false;
        }
    }
}
