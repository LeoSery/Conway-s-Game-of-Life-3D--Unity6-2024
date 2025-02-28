using System.Collections;

using UnityEngine;

using Unity.Mathematics;

public class CellInteractionController : MonoBehaviour
{
    #region Public Fields
    public float interactionDistance = 10f;
    public Color highlightColor = Color.yellow;
    #endregion

    #region Private Fields
    private Camera mainCamera;
    private Grid grid;
    private VisualGrid visualGrid;
    private Vector3 gridOffset;
    private Vector3Int? lastHighlightedCell;

    private int lastGridSize = -1;
    private Vector3 cachedRayOrigin = new(0.5f, 0.5f, 0);
    private float lastUpdateTime = 0f;
    private const float UPDATE_THROTTLE = 0.05f;
    #endregion

    #region Unity Lifecycle Methods

    private void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
            return;
        }

        StartCoroutine(WaitForGameManager());

        visualGrid = GameManager.Instance.visualGrid;
    }

    private void OnDisable()
    {
        UnsubscribeToEvents();
    }
    #endregion

    #region Public Methods
    public void ShowLayer()
    {
        visualGrid.ShowLayer();
        UpdateCellHighlight();
    }

    public void HideLayer()
    {
        visualGrid.HideLayer();
        UpdateCellHighlight();
    }
    #endregion

    #region Private Methods
    private IEnumerator WaitForGameManager()
    {
        while (GameManager.Instance == null || GameManager.Instance.Grid == null || GameManager.Instance.visualGrid == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        grid = GameManager.Instance.Grid;
        visualGrid = GameManager.Instance.visualGrid;

        UpdateGridOffset();

        if (InputManager.Instance != null)
        {
            SubscribeToEvents();
        }
        else
        {
            Debug.LogError("InputManager instance is null. Make sure it's initialized before CellInteractionController.");
        }
    }

    private void UpdateGridOffset()
    {
        int currentGridSize = GameManager.Instance.gridSize;

        if (currentGridSize != lastGridSize)
        {
            lastGridSize = currentGridSize;
            float cellSize = GameManager.Instance.CellSize;
            gridOffset.Set(
                currentGridSize / 2f * cellSize,
                currentGridSize / 2f * cellSize,
                currentGridSize / 2f * cellSize
            );

            lastHighlightedCell = null;
        }
    }

    private void SubscribeToEvents()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPlaceCell += PlaceCell;
            InputManager.Instance.OnRemoveCell += RemoveCell;

            InputManager.Instance.OnMove += HandleMovement;
            InputManager.Instance.OnMouseLook += HandleMouseLook;

            InputManager.Instance.OnShowLayer += ShowLayer;
            InputManager.Instance.OnHideLayer += HideLayer;
        }
    }

    private void UnsubscribeToEvents()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPlaceCell -= PlaceCell;
            InputManager.Instance.OnRemoveCell -= RemoveCell;

            InputManager.Instance.OnMove -= HandleMovement;
            InputManager.Instance.OnMouseLook -= HandleMouseLook;

            InputManager.Instance.OnShowLayer -= ShowLayer;
            InputManager.Instance.OnHideLayer -= HideLayer;
        }
    }

    private void UpdateCellHighlight()
    {
        float currentTime = Time.time;
        if (currentTime - lastUpdateTime < UPDATE_THROTTLE)
            return;

        lastUpdateTime = currentTime;

        UpdateGridOffset();

        Ray ray = mainCamera.ViewportPointToRay(cachedRayOrigin);
        Vector3Int? targetCell = FindTargetCell(ray);

        if (targetCell.HasValue)
        {
            if (!targetCell.Equals(lastHighlightedCell))
            {
                visualGrid.UnhighlightCell();
                visualGrid.HighlightCell(targetCell.Value);
                lastHighlightedCell = targetCell;
            }
        }
        else if (lastHighlightedCell.HasValue)
        {
            visualGrid.UnhighlightCell();
            lastHighlightedCell = null;
        }
    }

    private Vector3Int? FindTargetCell(Ray _ray)
    {
        int gridSize = GameManager.Instance.gridSize;
        float cellSize = GameManager.Instance.CellSize;
        int currentVisibleLayer = visualGrid.CurrentVisibleLayer;

        Vector3 gridMin = Vector3.zero - gridOffset;
        Vector3 gridMax = new Vector3(gridSize * cellSize, gridSize * cellSize, gridSize * cellSize) - gridOffset;

        float planeY = currentVisibleLayer * cellSize - gridOffset.y;

        if (Mathf.Abs(_ray.direction.y) < 0.0001f)
        {
            return null;
        }

        float t = (planeY - _ray.origin.y) / _ray.direction.y;

        if (t < 0)
        {
            return null;
        }

        Vector3 hitPoint = _ray.origin + _ray.direction * t;

        if (hitPoint.x >= gridMin.x && hitPoint.x < gridMax.x &&
            hitPoint.z >= gridMin.z && hitPoint.z < gridMax.z)
        {
            int3 cell = WorldToCellPosition(hitPoint);

            cell.y = currentVisibleLayer;

            if (IsValidCell(cell))
            {
                return new Vector3Int(cell.x, cell.y, cell.z);
            }
        }

        return null;
    }

    private int3 WorldToCellPosition(Vector3 _worldPosition)
    {
        Vector3 localPosition = _worldPosition + gridOffset;
        return new int3(
            Mathf.FloorToInt(localPosition.x),
            Mathf.FloorToInt(localPosition.y),
            Mathf.FloorToInt(localPosition.z)
        );
    }

    private bool IsValidCell(int3 _cellPosition)
    {
        return _cellPosition.x >= 0 && _cellPosition.x < grid.GridSize &&
               _cellPosition.y >= 0 && _cellPosition.y < grid.GridSize &&
               _cellPosition.z >= 0 && _cellPosition.z < grid.GridSize;
    }

    private void HandleMovement(Vector3 _movement)
    {
        UpdateCellHighlight();
    }

    private void HandleMouseLook(Vector2 _mouseDelta)
    {
        UpdateCellHighlight();
    }

    private void PlaceCell()
    {
        if (lastHighlightedCell.HasValue)
        {
            Vector3Int cellPosition = lastHighlightedCell.Value;
            GameManager.Instance.CreateCell(new int3(cellPosition.x, cellPosition.y, cellPosition.z));
        }
    }

    private void RemoveCell()
    {
        if (lastHighlightedCell.HasValue)
        {
            Vector3Int cellPosition = lastHighlightedCell.Value;
            GameManager.Instance.DestroyCell(new int3(cellPosition.x, cellPosition.y, cellPosition.z));
        }
    }
    #endregion
}
