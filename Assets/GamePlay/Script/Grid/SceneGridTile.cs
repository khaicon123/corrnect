using Corrnect.Core;
using UnityEngine;

namespace Corrnect.Grid
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SceneGridTile : MonoBehaviour
    {
        public Vector2Int GridPosition;
        public UnitType UnitType;

        private SpriteRenderer spriteRenderer;

        public void Initialize(SpriteRenderer renderer)
        {
            spriteRenderer = renderer;
            UpdateSpriteColor();
        }

        private void OnValidate()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            UpdateSpriteColor();
            gameObject.name = $"Tile_{GridPosition.x}_{GridPosition.y}_{UnitType}";
        }

        private void UpdateSpriteColor()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer == null)
                return;

            spriteRenderer.color = GetColorForUnitType(UnitType);
        }

        private Color GetColorForUnitType(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Left => Color.cyan,
                UnitType.Right => Color.magenta,
                UnitType.Up => Color.green,
                UnitType.Down => Color.blue,
                UnitType.Horizontal => new Color(0.4f, 0.8f, 0.9f),
                UnitType.Vertical => new Color(0.9f, 0.5f, 0.7f),
                UnitType.Free => Color.white,
                UnitType.DangerMoving => new Color(0.9f, 0.25f, 0.25f),
                UnitType.DangerStatic => new Color(0.9f, 0.6f, 0.2f),
                _ => Color.white,
            };
        }
    }
}
