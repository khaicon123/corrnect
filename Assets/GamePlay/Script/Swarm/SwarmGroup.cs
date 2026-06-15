using System.Collections.Generic;
using System.Linq;
using Corrnect.Core;
using Corrnect.Grid;
using UnityEngine;

namespace Corrnect.Swarm
{
    public class SwarmGroup : MonoBehaviour
    {
        [SerializeField] private Transform nodesContainer;
        [SerializeField] private Transform connectionsContainer;

        private static Sprite _sharedNodeSprite;
        private static Sprite _sharedLineSprite;

        private readonly List<SwarmNode> _nodes = new();
        private readonly List<GameObject> _connectionSegments = new();
        private GridManager _grid;
        private Vector2Int _gridPosition;

        public Vector2Int GridPosition => _gridPosition;
        public int NodeCount => _nodes.Count;
        public bool IsCompoundUnit => _nodes.Count > 1;
        public IReadOnlyList<SwarmNode> Nodes => _nodes;
        public IEnumerable<UnitType> MemberTypes => _nodes.Select(node => node.UnitType);

        public void Initialize(GridManager grid, Vector2Int startPosition, UnitType unitType)
        {
            _grid = grid;
            ClearNodes();
            EnsureStructure();
            AddNode(unitType);
            EnsureCollider();
            SetGridPosition(startPosition);
            FinalizeAsOneUnit();
        }

        public bool CanMove(Direction direction)
        {
            return _grid != null && CanMove(direction, _grid);
        }

        public bool CanMove(Direction direction, GridManager grid)
        {
            if (!UnitMovementRules.CanGroupMove(MemberTypes, direction))
                return false;

            return CanMoveToGridPosition(_gridPosition + direction.ToOffset(), grid);
        }

        public bool CanMoveToGridPosition(Vector2Int targetGridPosition, GridManager grid)
        {
            var gridOffset = targetGridPosition - _gridPosition;

            foreach (var node in _nodes)
            {
                var nodeGrid = grid.WorldToGrid(node.transform.position);
                var targetNodeGrid = nodeGrid + gridOffset;

                if (!grid.IsWalkable(targetNodeGrid))
                    return false;
            }

            return true;
        }

        public HashSet<Vector2Int> GetOccupiedGridCells(GridManager grid)
        {
            var cells = new HashSet<Vector2Int>();

            foreach (var node in _nodes)
                cells.Add(grid.WorldToGrid(node.transform.position));

            return cells;
        }

        public void Absorb(SwarmGroup other, GridManager grid)
        {
            EnsureStructure();

            var savedWorldPositions = other._nodes
                .Select(node => node.transform.position)
                .ToList();

            for (var i = 0; i < other._nodes.Count; i++)
            {
                var node = other._nodes[i];
                node.transform.SetParent(nodesContainer, true);
                node.transform.position = savedWorldPositions[i];
                _nodes.Add(node);
            }

            other._nodes.Clear();
            SyncAnchorFromNodes(grid);
            FinalizeAsOneUnit();
        }

        public void FinalizeAsOneUnit()
        {
            EnsureStructure();

            gameObject.name = _nodes.Count <= 1
                ? $"Unit_{_nodes.FirstOrDefault()?.UnitType.ToString() ?? "Empty"}"
                : $"SwarmUnit_{_nodes.Count}";

            var isMerged = _nodes.Count > 1;
            foreach (var node in _nodes)
                node.SetPartOfMergedUnit(isMerged);

            RefreshConnections();
            UpdateColliderSize();
        }

        public void SetGridPosition(Vector2Int gridPosition)
        {
            _gridPosition = gridPosition;

            if (_grid != null)
                transform.position = _grid.GridToWorld(gridPosition);

            RefreshConnections();
        }

        private void SyncAnchorFromNodes(GridManager grid)
        {
            if (_nodes.Count == 0)
                return;

            var savedWorldPositions = _nodes.Select(node => node.transform.position).ToList();
            var anchorCell = GetOccupiedGridCells(grid)
                .OrderBy(cell => cell.x)
                .ThenBy(cell => cell.y)
                .First();

            _gridPosition = anchorCell;
            transform.position = grid.GridToWorld(anchorCell);

            for (var i = 0; i < _nodes.Count; i++)
                _nodes[i].transform.position = savedWorldPositions[i];
        }

        private SwarmNode AddNode(UnitType unitType)
        {
            EnsureStructure();

            var nodeObject = new GameObject($"Node_{unitType}");
            nodeObject.transform.SetParent(nodesContainer, false);

            var node = nodeObject.AddComponent<SwarmNode>();
            var nodeSize = _grid != null ? _grid.CellSize * 0.32f : 0.32f;
            node.Initialize(unitType, nodeSize, GetSharedNodeSprite());
            node.transform.localPosition = Vector3.zero;
            _nodes.Add(node);
            return node;
        }

        private void ClearNodes()
        {
            ClearConnections();

            foreach (var node in _nodes)
            {
                if (node != null)
                    Destroy(node.gameObject);
            }

            _nodes.Clear();
        }

        private void EnsureStructure()
        {
            if (nodesContainer == null)
            {
                var containerObject = transform.Find("Nodes");
                nodesContainer = containerObject != null
                    ? containerObject
                    : new GameObject("Nodes").transform;
                nodesContainer.SetParent(transform, false);
            }

            if (connectionsContainer == null)
            {
                var containerObject = transform.Find("Connections");
                connectionsContainer = containerObject != null
                    ? containerObject
                    : new GameObject("Connections").transform;
                connectionsContainer.SetParent(transform, false);
            }
        }

        private void RefreshConnections()
        {
            ClearConnections();

            if (_nodes.Count < 2)
                return;

            for (var i = 0; i < _nodes.Count; i++)
            {
                var from = _nodes[i].transform.position;
                var to = _nodes[(i + 1) % _nodes.Count].transform.position;
                CreateConnectionSegment(from, to);
            }
        }

        private void CreateConnectionSegment(Vector3 from, Vector3 to)
        {
            var segment = new GameObject("Link");
            segment.transform.SetParent(connectionsContainer, true);

            var renderer = segment.AddComponent<SpriteRenderer>();
            renderer.sprite = GetSharedLineSprite();
            renderer.color = new Color(1f, 0.88f, 0.3f, 0.95f);
            renderer.sortingOrder = 2;

            var delta = to - from;
            var length = delta.magnitude;
            if (length < 0.001f)
                return;

            segment.transform.position = (from + to) * 0.5f;
            segment.transform.right = delta.normalized;
            segment.transform.localScale = new Vector3(length, (_grid != null ? _grid.CellSize : 1f) * 0.12f, 1f);

            _connectionSegments.Add(segment);
        }

        private void ClearConnections()
        {
            foreach (var segment in _connectionSegments)
            {
                if (segment != null)
                    Destroy(segment);
            }

            _connectionSegments.Clear();
        }

        private void EnsureCollider()
        {
            var collider = GetComponent<BoxCollider2D>();
            if (collider == null)
                collider = gameObject.AddComponent<BoxCollider2D>();

            collider.isTrigger = true;
            UpdateColliderSize();
        }

        private void UpdateColliderSize()
        {
            var collider = GetComponent<BoxCollider2D>();
            if (collider == null || _nodes.Count == 0)
                return;

            if (_nodes.Count == 1)
            {
                var size = _grid != null ? _grid.CellSize * 0.95f : 0.95f;
                collider.offset = Vector2.zero;
                collider.size = Vector2.one * size;
                return;
            }

            var min = _nodes[0].transform.localPosition;
            var max = min;

            for (var i = 1; i < _nodes.Count; i++)
            {
                var local = _nodes[i].transform.localPosition;
                min = Vector3.Min(min, local);
                max = Vector3.Max(max, local);
            }

            var padding = (_grid != null ? _grid.CellSize : 1f) * 0.35f;
            var center = (min + max) * 0.5f;
            var boundsSize = max - min + Vector3.one * padding;

            collider.offset = new Vector2(center.x, center.y);
            collider.size = new Vector2(Mathf.Max(boundsSize.x, padding), Mathf.Max(boundsSize.y, padding));
        }

        private static Sprite GetSharedNodeSprite()
        {
            if (_sharedNodeSprite != null)
                return _sharedNodeSprite;

            var texture = new Texture2D(16, 16);
            var center = new Vector2(8f, 8f);
            const float radius = 7f;

            for (var y = 0; y < 16; y++)
            {
                for (var x = 0; x < 16; x++)
                {
                    var distance = Vector2.Distance(new Vector2(x, y), center);
                    texture.SetPixel(x, y, distance <= radius ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            _sharedNodeSprite = Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
            return _sharedNodeSprite;
        }

        private static Sprite GetSharedLineSprite()
        {
            if (_sharedLineSprite != null)
                return _sharedLineSprite;

            var texture = new Texture2D(4, 4);
            for (var y = 0; y < 4; y++)
            {
                for (var x = 0; x < 4; x++)
                    texture.SetPixel(x, y, Color.white);
            }

            texture.Apply();
            _sharedLineSprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            return _sharedLineSprite;
        }

        private void Reset()
        {
            EnsureStructure();
        }
    }
}
