using Corrnect.Core;
using UnityEngine;

namespace Corrnect.Swarm
{
    public class SwarmNode : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public UnitType UnitType { get; private set; }
        public bool IsPartOfMergedUnit { get; private set; }

        public void Initialize(UnitType unitType, float size, Sprite sprite)
        {
            UnitType = unitType;
            IsPartOfMergedUnit = false;

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            spriteRenderer.sprite = sprite;
            spriteRenderer.color = GetColorForType(unitType, IsPartOfMergedUnit);
            spriteRenderer.sortingOrder = 3;
            transform.localScale = Vector3.one * size;
        }

        public void SetPartOfMergedUnit(bool isMerged)
        {
            IsPartOfMergedUnit = isMerged;
            if (spriteRenderer != null)
                spriteRenderer.color = GetColorForType(UnitType, IsPartOfMergedUnit);
        }

        public static Color GetColorForType(UnitType unitType, bool isMerged = false)
        {
            var baseColor = unitType switch
            {
                UnitType.Horizontal => new Color(0.35f, 0.65f, 0.95f),
                UnitType.Vertical => new Color(0.9f, 0.4f, 0.55f),
                UnitType.Free => new Color(0.35f, 0.85f, 0.45f),
                _ => Color.white
            };

            if (!isMerged)
                return baseColor;

            return new Color(baseColor.r * 0.9f, baseColor.g * 0.9f, baseColor.b * 0.9f, 0.9f);
        }

        private void Reset()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
}
