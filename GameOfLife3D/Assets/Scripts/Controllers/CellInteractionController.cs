using UnityEngine;
using Unity.Mathematics;

public class CellInteractionController : MonoBehaviour
{
    public float interactionDistance = 10f;
    public Color highlightColor = Color.yellow;

    private Camera mainCamera;
    private Grid grid;
    private VisualGrid visualGrid;
    private GameObject highlightedCell;
    private Vector3 gridWorldPosition;
    private Vector3 gridOffset;

    private Vector3Int? lastHighlightedCell;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
            return;
        }

        StartCoroutine(WaitForGameManager());

        gridOffset = new Vector3(-4.5f, -4.5f, -4.5f);
    }

    private System.Collections.IEnumerator WaitForGameManager()
    {
        while (GameManager.Instance == null || GameManager.Instance.Grid == null || GameManager.Instance.visualGrid == null)
        {
            Debug.Log("Waiting for GameManager, Grid, and VisualGrid to be initialized...");
            yield return new WaitForSeconds(0.1f);
        }

        grid = GameManager.Instance.Grid;
        visualGrid = GameManager.Instance.visualGrid;
        gridWorldPosition = visualGrid.transform.position;
        gridOffset = new Vector3(-5f, -5f, 0f);
        Debug.Log($"Grid reference obtained successfully. Grid world position: {gridWorldPosition}, Grid offset: {gridOffset}");

        if (InputManager.Instance != null)
        {
            SubscribeToEvents();
        }
        else
        {
            Debug.LogError("InputManager instance is null. Make sure it's initialized before CellInteractionController.");
        }
    }

    private void UpdateCellHighlight()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3Int? targetCell = FindTargetCell(ray);

        if (targetCell.HasValue && !targetCell.Equals(lastHighlightedCell))
        {
            if (lastHighlightedCell.HasValue)
            {
                visualGrid.UnhighlightCell();
            }
            visualGrid.HighlightCell(targetCell.Value);
            lastHighlightedCell = targetCell;
            Debug.Log($"Highlighting cell at position: {targetCell.Value}");
        }
        else if (!targetCell.HasValue && lastHighlightedCell.HasValue)
        {
            visualGrid.UnhighlightCell();
            lastHighlightedCell = null;
        }
    }

    private Vector3Int? FindTargetCell(Ray _ray)
    {
        Vector3 gridMin = gridOffset;
        Vector3 gridMax = -gridOffset;

        if (!IntersectRayBox(_ray, gridMin, gridMax, out float tMin, out float tMax))
        {
            return null;
        }

        Vector3 entryPoint = _ray.GetPoint(tMin);
        Vector3 currentPoint = entryPoint;
        Vector3 step = _ray.direction * 0.1f;

        while (IsInsideGrid(currentPoint))
        {
            int3 cellPosition = WorldToCellPosition(currentPoint);

            if (IsValidCell(cellPosition))
            {
                return new Vector3Int(cellPosition.x, cellPosition.y, cellPosition.z);
            }

            currentPoint += step;
        }

        return null;
    }

    private bool IntersectRayBox(Ray _ray, Vector3 _boxMin, Vector3 _boxMax, out float _tMin, out float _tMax)
    {
        Vector3 invDir = new Vector3(1f / _ray.direction.x, 1f / _ray.direction.y, 1f / _ray.direction.z);
        Vector3 t1 = Vector3.Scale((_boxMin - _ray.origin), invDir);
        Vector3 t2 = Vector3.Scale((_boxMax - _ray.origin), invDir);

        _tMin = Mathf.Max(Mathf.Max(Mathf.Min(t1.x, t2.x), Mathf.Min(t1.y, t2.y)), Mathf.Min(t1.z, t2.z));
        _tMax = Mathf.Min(Mathf.Min(Mathf.Max(t1.x, t2.x), Mathf.Max(t1.y, t2.y)), Mathf.Max(t1.z, t2.z));

        return _tMax >= _tMin && _tMax >= 0;
    }

    private bool IsInsideGrid(Vector3 _point)
    {
        Vector3 localPoint = _point - (gridWorldPosition + gridOffset);
        return localPoint.x >= 0 && localPoint.x < grid.GridSize &&
               localPoint.y >= 0 && localPoint.y < grid.GridSize &&
               localPoint.z >= 0 && localPoint.z < grid.GridSize;
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

    private void HighlightCell(int3 _position)
    {
        ClearHighlight();

        Vector3 worldPosition = gridOffset + new Vector3(
            _position.x,
            _position.y,
            _position.z
        );

        highlightedCell = GameObject.CreatePrimitive(PrimitiveType.Cube);
        highlightedCell.transform.position = worldPosition;
        highlightedCell.transform.localScale = Vector3.one * 0.9f;

        Renderer renderer = highlightedCell.GetComponent<Renderer>();
        renderer.material.color = highlightColor;
        renderer.material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);

        Debug.Log($"Created highlight cube at world position: {highlightedCell.transform.position}, Grid position: {_position}");
    }

    private void ClearHighlight()
    {
        if (highlightedCell != null)
        {
            Destroy(highlightedCell);
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
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPlaceCell -= PlaceCell;
            InputManager.Instance.OnRemoveCell -= RemoveCell;

            InputManager.Instance.OnMove -= HandleMovement;
            InputManager.Instance.OnMouseLook -= HandleMouseLook;
        }
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
        if (highlightedCell != null)
        {
            int3 cellPosition = WorldToCellPosition(highlightedCell.transform.position);

            cellPosition = new int3(
                Mathf.Clamp(cellPosition.x, 0, GameManager.Instance.gridSize - 1),
                Mathf.Clamp(cellPosition.y, 0, GameManager.Instance.gridSize - 1),
                Mathf.Clamp(cellPosition.z, 0, GameManager.Instance.gridSize - 1)
            );

            GameManager.Instance.CreateCell(cellPosition);
            Debug.Log($"Placing cell at position: {cellPosition}");
        }
    }

    private void RemoveCell()
    {
        if (highlightedCell != null)
        {
            int3 cellPosition = WorldToCellPosition(highlightedCell.transform.position);
            GameManager.Instance.DestroyCell(cellPosition);
            Debug.Log($"Removing cell at position: {cellPosition}");
        }
    }
}
