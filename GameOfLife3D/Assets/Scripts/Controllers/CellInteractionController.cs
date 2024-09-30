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

        gridOffset = new Vector3(
        -GameManager.Instance.gridSize / 2f,
        -GameManager.Instance.gridSize / 2f,
        -GameManager.Instance.gridSize / 2f
        );

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
        gridOffset = new Vector3(-5f, -5f, 0f);

        if (InputManager.Instance != null)
        {
            SubscribeToEvents();
        }
        else
        {
            Debug.LogError("InputManager instance is null. Make sure it's initialized before CellInteractionController.");
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
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
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
        else
        {
            if (lastHighlightedCell.HasValue)
            {
                visualGrid.UnhighlightCell();
                lastHighlightedCell = null;
            }
        }
    }

    private Vector3Int? FindTargetCell(Ray _ray)
    {
        Vector3 gridMin = gridOffset;
        Vector3 gridMax = -gridOffset;

        if (IntersectRayBox(_ray, gridMin, gridMax, out float tMin, out float tMax))
        {
            Vector3 entryPoint = _ray.GetPoint(tMin);
            Vector3 exitPoint = _ray.GetPoint(tMax);

            Vector3 direction = (exitPoint - entryPoint).normalized;
            float distance = Vector3.Distance(entryPoint, exitPoint);

            for (float t = 0; t <= distance; t += 0.1f)
            {
                Vector3 point = entryPoint + direction * t;
                int3 cell = WorldToCellPosition(point);

                if (IsValidCell(cell))
                {
                    if (visualGrid.VisibleLayers == 1)
                    {
                        if (cell.y == 0)
                        {
                            return new Vector3Int(cell.x, cell.y, cell.z);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else if (cell.y < visualGrid.VisibleLayers)
                    {
                        return new Vector3Int(cell.x, cell.y, cell.z);
                    }
                }
            }
        }
        return null;
    }

    private bool IntersectRayBox(Ray _ray, Vector3 _boxMin, Vector3 _boxMax, out float _tMin, out float _tMax)
    {
        Vector3 invDir = new(1f / _ray.direction.x, 1f / _ray.direction.y, 1f / _ray.direction.z);
        Vector3 t1 = Vector3.Scale((_boxMin - _ray.origin), invDir);
        Vector3 t2 = Vector3.Scale((_boxMax - _ray.origin), invDir);

        _tMin = Mathf.Max(Mathf.Max(Mathf.Min(t1.x, t2.x), Mathf.Min(t1.y, t2.y)), Mathf.Min(t1.z, t2.z));
        _tMax = Mathf.Min(Mathf.Min(Mathf.Max(t1.x, t2.x), Mathf.Max(t1.y, t2.y)), Mathf.Max(t1.z, t2.z));

        return _tMax >= _tMin && _tMax >= 0;
    }

    private int3 WorldToCellPosition(Vector3 _worldPosition)
    {
        Vector3 localPosition = _worldPosition - gridOffset;
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
