using System.Collections.Generic;
using UnityEngine;

public class VisualGrid : MonoBehaviour
{
    [Header("Settings :")]
    public Color gridColor = new(0.5f, 0.5f, 0.5f, 0.2f);
    public Color highlightColor = Color.yellow;
    public float gridLineWidth = 0.02f;
    public float highlightLineWidth = 0.04f;

    private int gridSize;
    private float cellSize;
    private Vector3 gridOffset;
    private readonly List<LineRenderer> gridLines = new();
    private readonly List<LineRenderer> highlightLines = new();

    public void Initialize(int _size, float _cellSize)
    {
        gridSize = _size;
        cellSize = _cellSize;
        CreateGrid();
        CreateHighlightLines();
    }

    private void CreateGrid()
    {
        ClearExistingLines(gridLines);
        gridOffset = new Vector3(gridSize / 2f, gridSize / 2f, gridSize / 2f) * cellSize;

        for (int i = 0; i <= gridSize; i++)
        {
            float pos = i * cellSize;

            // X direction
            for (int x = 0; x <= gridSize; x++)
            {
                CreateGridLine(new Vector3(0, x * cellSize, pos) - gridOffset,
                               new Vector3(gridSize * cellSize, x * cellSize, pos) - gridOffset);
            }

            // Y direction
            for (int y = 0; y <= gridSize; y++)
            {
                CreateGridLine(new Vector3(y * cellSize, 0, pos) - gridOffset,
                               new Vector3(y * cellSize, gridSize * cellSize, pos) - gridOffset);
            }

            // Z direction
            for (int z = 0; z <= gridSize; z++)
            {
                CreateGridLine(new Vector3(pos, z * cellSize, 0) - gridOffset,
                               new Vector3(pos, z * cellSize, gridSize * cellSize) - gridOffset);
            }
        }
    }

    private void CreateHighlightLines()
    {
        ClearExistingLines(highlightLines);
        for (int i = 0; i < 12; i++) // 12 edges for a cube
        {
            LineRenderer lr = CreateLine(Vector3.zero, Vector3.zero, highlightColor, highlightLineWidth);
            lr.gameObject.SetActive(false);
            highlightLines.Add(lr);
        }
    }

    private void CreateGridLine(Vector3 _start, Vector3 _end)
    {
        CreateLine(_start, _end, gridColor, gridLineWidth, gridLines);
    }

    private LineRenderer CreateLine(Vector3 _start, Vector3 _end, Color _color, float _width, List<LineRenderer> _lineList = null)
    {
        GameObject lineObj = new("GridLine");
        lineObj.transform.SetParent(this.transform);
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = _color;
        lr.startWidth = lr.endWidth = _width;

        lr.positionCount = 2;
        lr.SetPosition(0, _start);
        lr.SetPosition(1, _end);

        _lineList?.Add(lr);
        return lr;
    }

    public void HighlightCell(Vector3Int _cellPosition)
    {
        Vector3 start = new Vector3(_cellPosition.x, _cellPosition.y, _cellPosition.z) * cellSize - gridOffset;
        Vector3 end = start + Vector3.one * cellSize;

        Vector3[] positions = new Vector3[]
        {
            start,
            new Vector3(end.x, start.y, start.z),
            new Vector3(start.x, end.y, start.z),
            new Vector3(start.x, start.y, end.z),
            new Vector3(end.x, end.y, start.z),
            new Vector3(end.x, start.y, end.z),
            new Vector3(start.x, end.y, end.z),
            end
        };

        for (int i = 0; i < 12; i++)
        {
            LineRenderer lr = highlightLines[i];
            lr.SetPosition(0, positions[CubeEdges[i, 0]]);
            lr.SetPosition(1, positions[CubeEdges[i, 1]]);
            lr.gameObject.SetActive(true);
        }
    }

    public void UnhighlightCell()
    {
        foreach (var lr in highlightLines)
        {
            lr.gameObject.SetActive(false);
        }
    }

    public void UpdateGridSize(int _newSize, float _newCellSize)
    {
        gridSize = _newSize;
        cellSize = _newCellSize;
        CreateGrid();
        CreateHighlightLines();
    }

    private void ClearExistingLines(List<LineRenderer> _lines)
    {
        foreach (var lr in _lines)
        {
            if (lr != null)
            {
                Destroy(lr.gameObject);
            }
        }
        _lines.Clear();
    }

    private static readonly int[,] CubeEdges = new int[,]
    {
        {0, 1}, {1, 4}, {4, 2},
        {2, 0}, {0, 3}, {1, 5}, 
        {4, 7}, {2, 6}, {3, 5},
        {5, 7}, {7, 6}, {6, 3} 
    };
}
