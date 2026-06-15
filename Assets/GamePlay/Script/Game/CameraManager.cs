using System.Linq;
using UnityEngine;

namespace Corrnect.Game
{
    [RequireComponent(typeof(Camera))]
    public class CameraManager : MonoBehaviour
    {
        [Header("Camera Target")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private bool useMainCamera = true;

        [Header("Scene Fit")]
        [SerializeField] private bool fitAllRenderers = true;
        [SerializeField] private bool fitSpecificTransforms;
        [SerializeField] private Transform[] focusTransforms;
        [SerializeField] private bool includeInactiveObjects;

        [Header("Level Fit")]
        [SerializeField] private bool fitLevelDefinition;
        [SerializeField] private Corrnect.Grid.LevelDefinition levelDefinition;
        [SerializeField] private Corrnect.Grid.GridManager gridManager;

        [Header("Padding & Limits")]
        [SerializeField] private Vector2 padding = new Vector2(1f, 1f);
        [SerializeField] private float minOrthographicSize = 3f;
        [SerializeField] private float maxOrthographicSize = 50f;

        [Header("Size Mode")]
        [SerializeField] private bool sizeIsWidth = true;

        [Header("Motion")]
        [SerializeField] private bool smoothMove = true;
        [SerializeField] private float smoothTime = 0.25f;

        private Vector3 _velocity;

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = useMainCamera ? Camera.main : GetComponent<Camera>();

            if (targetCamera == null)
                targetCamera = GetComponent<Camera>();
        }

        private void Start()
        {
            if (targetCamera == null)
            {
                Debug.LogError("CameraManager requires a Camera reference or a Main Camera in the scene.");
                enabled = false;
                return;
            }

            if (!targetCamera.orthographic)
                targetCamera.orthographic = true;

            if (fitLevelDefinition)
            {
                if (levelDefinition == null || gridManager == null)
                {
                    var controller = FindObjectOfType<GameController>();
                    if (controller != null)
                    {
                        if (levelDefinition == null)
                            levelDefinition = controller.CurrentLevelDefinition;

                        if (gridManager == null)
                            gridManager = controller.Grid;
                    }
                }

                if (levelDefinition != null)
                {
                    FitToLevelDefinition(levelDefinition, gridManager, true);
                    return;
                }
            }

            FitToScene();
        }

        public void FitToScene()
        {
            if (fitSpecificTransforms && focusTransforms != null && focusTransforms.Length > 0)
            {
                var bounds = GetBoundsFromTransforms(focusTransforms);
                FitToBounds(bounds);
                return;
            }

            if (fitAllRenderers)
            {
                var renderers = FindObjectsOfType<Renderer>(includeInactiveObjects);
                if (TryGetBounds(renderers, out var bounds))
                {
                    FitToBounds(bounds);
                    return;
                }
            }

            Debug.LogWarning("CameraManager could not determine scene bounds. Assign Focus Transforms or enable Renderer fitting.");
        }

        public void FitToBounds(Bounds bounds, bool immediate = false)
        {
            if (targetCamera == null)
                return;

            var targetCenter = new Vector3(0f, 0f, targetCamera.transform.position.z);
            var halfHeight = bounds.extents.y + padding.y;
            var halfWidth = bounds.extents.x + padding.x;

            if (sizeIsWidth)
            {
                var fullWidth = halfWidth * 2f;
                var fullHeight = halfHeight * 2f;
                var requiredVisibleWidth = Mathf.Max(fullWidth, fullHeight * targetCamera.aspect);
                var requiredOrtho = requiredVisibleWidth / (2f * targetCamera.aspect);
                requiredOrtho = Mathf.Clamp(requiredVisibleWidth, minOrthographicSize, maxOrthographicSize) / (2f * targetCamera.aspect);

                Debug.Log($"CameraManager.FitToBounds(widthMode) -> center={bounds.center}, fullWidth={fullWidth:F2}, fullHeight={fullHeight:F2}, aspect={targetCamera.aspect:F2}, requiredVisibleWidth={requiredVisibleWidth:F2}, targetOrtho={requiredOrtho:F2}");

                if (immediate || !smoothMove)
                {
                    targetCamera.transform.position = targetCenter;
                    targetCamera.orthographicSize = requiredOrtho;
                    return;
                }

                targetCamera.transform.position = Vector3.SmoothDamp(targetCamera.transform.position, targetCenter, ref _velocity, smoothTime);
                targetCamera.orthographicSize = Mathf.Lerp(targetCamera.orthographicSize, requiredOrtho, Time.deltaTime * 8f);
                return;
            }

            var requiredSize = Mathf.Max(halfHeight, halfWidth / targetCamera.aspect);
            requiredSize = Mathf.Clamp(requiredSize, minOrthographicSize, maxOrthographicSize);

            Debug.Log($"CameraManager.FitToBounds -> center={bounds.center}, halfHeight={halfHeight:F2}, halfWidth={halfWidth:F2}, aspect={targetCamera.aspect:F2}, requiredSize={requiredSize:F2}, currentSize={targetCamera.orthographicSize:F2}");

            if (immediate || !smoothMove)
            {
                targetCamera.transform.position = targetCenter;
                targetCamera.orthographicSize = requiredSize;
                return;
            }

            targetCamera.transform.position = Vector3.SmoothDamp(targetCamera.transform.position, targetCenter, ref _velocity, smoothTime);
            targetCamera.orthographicSize = Mathf.Lerp(targetCamera.orthographicSize, requiredSize, Time.deltaTime * 8f);
        }

        public void FitToLevelDefinition(Corrnect.Grid.LevelDefinition levelDefinition, Corrnect.Grid.GridManager gridManager = null, bool immediate = false)
        {
            if (targetCamera == null || levelDefinition == null)
                return;
            var cellSize = gridManager != null ? gridManager.CellSize : 1f;
            var levelCenter = gridManager != null ? gridManager.transform.position : Vector3.zero;

            var levelWidth = levelDefinition.Width * cellSize;
            var levelHeight = levelDefinition.Height * cellSize;

            // Apply padding to level dimensions
            var paddedWidth = levelWidth + padding.x * 2f;
            var paddedHeight = levelHeight + padding.y * 2f;

            var requiredOrthoSize = 0f;
            if (sizeIsWidth)
            {
                var requiredVisibleWidth = Mathf.Max(paddedWidth, paddedHeight * targetCamera.aspect);
                requiredVisibleWidth = Mathf.Clamp(requiredVisibleWidth, minOrthographicSize, maxOrthographicSize);
                requiredOrthoSize = requiredVisibleWidth / (2f * targetCamera.aspect);

                Debug.Log($"CameraManager.FitToLevelDefinition(widthMode) -> levelWxH={levelWidth:F2}x{levelHeight:F2}, paddedWxH={paddedWidth:F2}x{paddedHeight:F2}, cellSize={cellSize:F2}, aspect={targetCamera.aspect:F2}, requiredVisibleWidth={requiredVisibleWidth:F2}, targetOrtho={requiredOrthoSize:F2}");
            }
            else
            {
                // size is orthographic half-height (orthographicSize)
                // constraints:
                // - level width must fit into visible width: paddedWidth <= 2 * orthoSize * aspect
                // - level height must fit into visible height: paddedHeight <= 2 * orthoSize
                var requiredOrthoFromWidth = paddedWidth / (2f * targetCamera.aspect);
                var requiredOrthoFromHeight = paddedHeight / 2f;

                requiredOrthoSize = Mathf.Max(requiredOrthoFromWidth, requiredOrthoFromHeight);
                requiredOrthoSize = Mathf.Clamp(requiredOrthoSize, minOrthographicSize, maxOrthographicSize);

                Debug.Log($"CameraManager.FitToLevelDefinition -> levelWxH={levelWidth:F2}x{levelHeight:F2}, paddedWxH={paddedWidth:F2}x{paddedHeight:F2}, cellSize={cellSize:F2}, aspect={targetCamera.aspect:F2}, orthoFromWidth={requiredOrthoFromWidth:F2}, orthoFromHeight={requiredOrthoFromHeight:F2}, targetOrtho={requiredOrthoSize:F2}");
            }

            // Keep camera position fixed at origin in x/y and preserve current z
            var targetCenter = new Vector3(0f, 0f, targetCamera.transform.position.z);

            if (immediate || !smoothMove)
            {
                targetCamera.transform.position = targetCenter;
                targetCamera.orthographicSize = requiredOrthoSize;
                return;
            }

            targetCamera.transform.position = Vector3.SmoothDamp(targetCamera.transform.position, targetCenter, ref _velocity, smoothTime);
            targetCamera.orthographicSize = Mathf.Lerp(targetCamera.orthographicSize, requiredOrthoSize, Time.deltaTime * 8f);
        }

        private static Bounds GetBoundsFromTransforms(Transform[] transforms)
        {
            var validTransforms = transforms.Where(t => t != null).ToArray();
            if (validTransforms.Length == 0)
                return new Bounds(Vector3.zero, Vector3.one * 2f);

            var bounds = new Bounds(validTransforms[0].position, Vector3.zero);

            foreach (var transform in validTransforms)
            {
                bounds.Encapsulate(transform.position);
            }

            return bounds;
        }

        private static bool TryGetBounds(Renderer[] renderers, out Bounds bounds)
        {
            bounds = default;
            if (renderers == null || renderers.Length == 0)
                return false;

            var visibleRenderers = renderers.Where(renderer => renderer != null).ToArray();
            if (visibleRenderers.Length == 0)
                return false;

            bounds = visibleRenderers[0].bounds;
            foreach (var renderer in visibleRenderers.Skip(1))
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return true;
        }
    }
}
