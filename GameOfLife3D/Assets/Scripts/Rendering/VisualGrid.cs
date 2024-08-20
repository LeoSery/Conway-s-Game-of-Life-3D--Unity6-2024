using System.Collections.Generic;

using UnityEngine;

public class VisualGrid : MonoBehaviour
{
    [Header("Setings :")]
    public Color gridColor = new(0.5f, 0.5f, 0.5f, 0.2f);
    public float lineWidth = 0.02f;

    private int gridSize;
    private float cellSize;
    private Vector3 gridOffset;
    private readonly List<LineRenderer> lineRenderers = new();

    public void Initialize(int _size, float _cellSize)
    {
        gridSize = _size;
        cellSize = _cellSize;
        CreateGrid();
    }

    private void CreateGrid()
    {
        ClearExistingLines();

        gridOffset = new Vector3(gridSize / 2f, gridSize / 2f, gridSize / 2f) * cellSize;

        for (int i = 0; i <= gridSize; i++)
        {
            float pos = i * cellSize;

            // X direction
            for (int x = 0; x <= gridSize; x++)
            {
                CreateLine(new Vector3(0, x * cellSize, pos) - gridOffset,
                           new Vector3(gridSize * cellSize, x * cellSize, pos) - gridOffset);
            }

            // Y direction
            for (int y = 0; y <= gridSize; y++)
            {
                CreateLine(new Vector3(y * cellSize, 0, pos) - gridOffset,
                           new Vector3(y * cellSize, gridSize * cellSize, pos) - gridOffset);
            }

            // Z direction
            for (int z = 0; z <= gridSize; z++)
            {
                CreateLine(new Vector3(pos, z * cellSize, 0) - gridOffset,
                           new Vector3(pos, z * cellSize, gridSize * cellSize) - gridOffset);
            }
        }
    }

    private void CreateLine(Vector3 _start, Vector3 _end)
    {
        GameObject lineObj = new("GridLine");
        lineObj.transform.SetParent(this.transform);
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = gridColor;
        lr.startWidth = lr.endWidth = lineWidth;

        lr.positionCount = 2;
        lr.SetPosition(0, _start);
        lr.SetPosition(1, _end);

        lineRenderers.Add(lr);
    }

    private void ClearExistingLines()
    {
        foreach (var lr in lineRenderers)
        {
            if (lr != null)
            {
                Destroy(lr.gameObject);
            }
        }
        lineRenderers.Clear();
    }

    public void UpdateGridSize(int _newSize, float _newCellSize)
    {
        gridSize = _newSize;
        cellSize = _newCellSize;
        CreateGrid();
    }
}
