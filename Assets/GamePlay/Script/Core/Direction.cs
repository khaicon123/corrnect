using UnityEngine;

namespace Corrnect.Core
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public static class DirectionExtensions
    {
        public static Vector2Int ToOffset(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => Vector2Int.up,
                Direction.Down => Vector2Int.down,
                Direction.Left => Vector2Int.left,
                Direction.Right => Vector2Int.right,
                _ => Vector2Int.zero
            };
        }

        public static int LeadingEdgeSortKey(this Direction direction, Vector2Int position)
        {
            return direction switch
            {
                Direction.Up => position.y,
                Direction.Down => -position.y,
                Direction.Left => position.x,
                Direction.Right => -position.x,
                _ => 0
            };
        }
    }
}
