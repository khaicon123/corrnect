using Corrnect.Core;
using UnityEngine;

namespace Corrnect.Grid
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Transform floorParent;
        [SerializeField] private Transform wallParent;
        [SerializeField] private GameObject floorPrefab;
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private Sprite floorSprite;
        [SerializeField] private Sprite wallSprite;

        private LevelDefinition _level;
        private Vector2Int _originOffset;

        public float CellSize => cellSize;
        public int Width => _level != null ? _level.Width : 0;
        public int Height => _level != null ? _level.Height : 0;

        public void Initialize(LevelDefinition level)
        {
            EnsureParents();
            _level = level;
            _originOffset = new Vector2Int(-level.Width / 2, -level.Height / 2);
            BuildVisuals();
        }

        public bool IsInBounds(Vector2Int position)
        {
            return position.x >= 0 && position.x < Width
                && position.y >= 0 && position.y < Height;
        }

        public CellType GetCell(Vector2Int position)
        {
            if (!IsInBounds(position))
                return CellType.Wall;

            return _level.GetCell(position.x, position.y);
        }

        public bool IsWalkable(Vector2Int position)
        {
            return IsInBounds(position) && GetCell(position) == CellType.Floor;
        }

        public Vector3 GridToWorld(Vector2Int gridPosition)
        {
            var centered = gridPosition + _originOffset;
            return transform.position + new Vector3(centered.x * cellSize, centered.y * cellSize, 0f);
        }

        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            var local = worldPosition - transform.position;
            var centered = new Vector2Int(
                Mathf.RoundToInt(local.x / cellSize),
                Mathf.RoundToInt(local.y / cellSize));
            return centered - _originOffset;
        }

        private void BuildVisuals()
        {
            ClearChildren(floorParent);
            ClearChildren(wallParent);

            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var position = new Vector2Int(x, y);
                    var cellType = GetCell(position);
                    var parent = cellType == CellType.Wall ? wallParent : floorParent;
                    CreateCellVisual(parent, position, cellType);
                }
            }
        }

        private void CreateCellVisual(Transform parent, Vector2Int gridPosition, CellType cellType)
        {
            var prefab = cellType == CellType.Wall ? wallPrefab : floorPrefab;
            GameObject cellObject;

            if (prefab != null)
            {
                cellObject = Instantiate(prefab, parent);
                cellObject.name = cellType == CellType.Wall ? "Wall" : "Floor";
            }
            else
            {
                cellObject = new GameObject(cellType == CellType.Wall ? "Wall" : "Floor");
                cellObject.transform.SetParent(parent, false);
                var renderer = cellObject.AddComponent<SpriteRenderer>();
                renderer.sprite = cellType == CellType.Wall ? wallSprite : floorSprite;
                renderer.sortingOrder = cellType == CellType.Wall ? 0 : -1;

                if (renderer.sprite == null)
                {
                    renderer.color = cellType == CellType.Wall
                        ? new Color(0.25f, 0.25f, 0.3f)
                        : new Color(0.15f, 0.17f, 0.2f);
                }

                cellObject.transform.localScale = Vector3.one * cellSize * 0.95f;
            }

            cellObject.transform.position = GridToWorld(gridPosition);
        }

        private static void ClearChildren(Transform parent)
        {
            if (parent == null)
                return;

            for (var i = parent.childCount - 1; i >= 0; i--)
                Destroy(parent.GetChild(i).gameObject);
        }

        private void Reset()
        {
            EnsureParents();
        }

        private void EnsureParents()
        {
            if (floorParent == null)
                floorParent = CreateChildTransform("Floors");

            if (wallParent == null)
                wallParent = CreateChildTransform("Walls");
        }

        private Transform CreateChildTransform(string name)
        {
            var child = new GameObject(name).transform;
            child.SetParent(transform, false);
            return child;
        }
    }
}
