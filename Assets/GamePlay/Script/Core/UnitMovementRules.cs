using System.Collections.Generic;

namespace Corrnect.Core
{
    public static class UnitMovementRules
    {
        public static bool CanMove(UnitType unitType, Direction direction)
        {
            return unitType switch
            {
                UnitType.Left => direction is Direction.Left,
                UnitType.Right => direction is Direction.Right,
                UnitType.Up => direction is Direction.Up,
                UnitType.Down => direction is Direction.Down,
                UnitType.Horizontal => direction is Direction.Left or Direction.Right,
                UnitType.Vertical => direction is Direction.Up or Direction.Down,
                UnitType.Free => true,
                _ => false
            };
        }

        /// <summary>
        /// Merged groups combine movement rules: if any member can move in a direction, the group can.
        /// </summary>
        public static bool CanGroupMove(IEnumerable<UnitType> memberTypes, Direction direction)
        {
            foreach (var unitType in memberTypes)
            {
                if (CanMove(unitType, direction))
                    return true;
            }

            return false;
        }
    }
}
