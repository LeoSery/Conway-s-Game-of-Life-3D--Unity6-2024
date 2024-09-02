using UnityEngine;
using Unity.Mathematics;
using System.Collections;

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

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
            return;
        }

        StartCoroutine(WaitForGameManager());
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
        gridOffset = new Vector3(-5f, -5f, 0f); // Ajustement pour le décalage observé
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

    private void Update()
    {
        if (grid != null)
        {
            UpdateCellHighlight();
        }
    }

    private void UpdateCellHighlight()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        int3? targetCell = FindTargetCell(ray);

        if (targetCell.HasValue)
        {
            HighlightCell(targetCell.Value);
            Debug.Log($"Highlighting cell at position: {targetCell.Value}");
        }
        else
        {
            ClearHighlight();
        }
    }

    private int3? FindTargetCell(Ray ray)
    {
        Vector3 gridMin = gridWorldPosition + gridOffset;
        Vector3 gridMax = gridMin + new Vector3(grid.GridSize, grid.GridSize, grid.GridSize);

        float tMin, tMax;
        if (!IntersectRayBox(ray, gridMin, gridMax, out tMin, out tMax))
        {
            return null; // Le rayon ne traverse pas la grille
        }

        Vector3 entryPoint = ray.GetPoint(tMin);
        Vector3 currentPoint = entryPoint;
        Vector3 step = ray.direction * 0.1f; // Petit pas pour avancer le long du rayon

        while (IsInsideGrid(currentPoint))
        {
            int3 cellPosition = WorldToCellPosition(currentPoint);

            if (IsValidCell(cellPosition))
            {
                return cellPosition;
            }

            currentPoint += step;
        }

        return null;
    }

    private bool IntersectRayBox(Ray ray, Vector3 boxMin, Vector3 boxMax, out float tMin, out float tMax)
    {
        Vector3 invDir = new Vector3(1f / ray.direction.x, 1f / ray.direction.y, 1f / ray.direction.z);
        Vector3 t1 = Vector3.Scale((boxMin - ray.origin), invDir);
        Vector3 t2 = Vector3.Scale((boxMax - ray.origin), invDir);

        tMin = Mathf.Max(Mathf.Max(Mathf.Min(t1.x, t2.x), Mathf.Min(t1.y, t2.y)), Mathf.Min(t1.z, t2.z));
        tMax = Mathf.Min(Mathf.Min(Mathf.Max(t1.x, t2.x), Mathf.Max(t1.y, t2.y)), Mathf.Max(t1.z, t2.z));

        return tMax >= tMin && tMax >= 0;
    }

    private bool IsInsideGrid(Vector3 point)
    {
        Vector3 localPoint = point - (gridWorldPosition + gridOffset);
        return localPoint.x >= 0 && localPoint.x < grid.GridSize &&
               localPoint.y >= 0 && localPoint.y < grid.GridSize &&
               localPoint.z >= 0 && localPoint.z < grid.GridSize;
    }

    private int3 WorldToCellPosition(Vector3 worldPosition)
    {
        Vector3 localPosition = worldPosition - (gridWorldPosition + gridOffset);
        return new int3(
            Mathf.FloorToInt(localPosition.x),
            Mathf.FloorToInt(localPosition.y),
            Mathf.FloorToInt(localPosition.z)
        );
    }

    private bool IsValidCell(int3 cellPosition)
    {
        return cellPosition.x >= 0 && cellPosition.x < grid.GridSize &&
               cellPosition.y >= 0 && cellPosition.y < grid.GridSize &&
               cellPosition.z >= 0 && cellPosition.z < grid.GridSize;
    }

    private void HighlightCell(int3 position)
    {
        ClearHighlight();

        Vector3 worldPosition = gridWorldPosition + gridOffset + new Vector3(position.x + 0.5f, position.y + 0.5f, position.z + 0.5f);
        highlightedCell = GameObject.CreatePrimitive(PrimitiveType.Cube);
        highlightedCell.transform.position = worldPosition;
        highlightedCell.transform.localScale = Vector3.one * 0.9f;

        Renderer renderer = highlightedCell.GetComponent<Renderer>();
        renderer.material.color = highlightColor;
        renderer.material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);

        Debug.Log($"Created highlight cube at world position: {highlightedCell.transform.position}, Grid position: {position}");
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
        InputManager.Instance.OnPlaceCell += PlaceCell;
        InputManager.Instance.OnRemoveCell += RemoveCell;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPlaceCell -= PlaceCell;
            InputManager.Instance.OnRemoveCell -= RemoveCell;
        }
    }

    private void PlaceCell()
    {
        if (highlightedCell != null)
        {
            int3 cellPosition = WorldToCellPosition(highlightedCell.transform.position);
            grid.SetAlive(cellPosition);
            Debug.Log($"Placing cell at position: {cellPosition}");
        }
    }

    private void RemoveCell()
    {
        if (highlightedCell != null)
        {
            int3 cellPosition = WorldToCellPosition(highlightedCell.transform.position);
            grid.RemoveCell(cellPosition);
            Debug.Log($"Removing cell at position: {cellPosition}");
        }
    }
}