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
    private readonly List<List<LineRenderer>> layerLines = new();
    private readonly List<LineRenderer> verticalLines = new();

    /// <summary>
    /// Represents the edges of a cube, where each pair of integers represents
    /// the indices of the vertices that are connected by an edge.
    /// </summary>
    /// <remarks>
    /// The cube vertices are numbered as follows:
    /// 
    ///        3 -------- 2
    ///       /|         /|
    ///      / |        / |
    ///     7 -------- 6  |
    ///     |  |       |  |
    ///     |  0 ------|- 1
    ///     | /        | /
    ///     |/         |/
    ///     4 -------- 5
    ///
    /// </remarks>
    private static readonly int[,] CubeEdges = new int[,]
    {
        {0, 1}, {1, 4}, {4, 2},
        {2, 0}, {0, 3}, {1, 5},
        {4, 7}, {2, 6}, {3, 5},
        {5, 7}, {7, 6}, {6, 3}
    };

    /// <summary>
    /// The number of visible layers in the grid.
    /// </summary>
    public int VisibleLayers { get; private set; }

    /// <summary>
    /// Initializes the visual grid with the specified size and cell size.
    /// </summary>
    /// <param name="_size">The size of the grid.</param>
    /// <param name="_cellSize">The size of each cell.</param>
    public void Initialize(int _size, float _cellSize)
    {
        gridSize = _size;
        cellSize = _cellSize;
        CreateGrid();
        CreateHighlightLines();
    }

    /// <summary>
    /// Updates the size and cell size of the visual grid.
    /// </summary>
    /// <param name="_newSize">The new size of the grid.</param>
    /// <param name="_newCellSize">The new size of each cell.</param>
    public void UpdateGridSize(int _newSize, float _newCellSize)
    {
        gridSize = _newSize;
        cellSize = _newCellSize;
        CreateGrid();
        CreateHighlightLines();
    }

    /// <summary>
    /// Highlights the cell at the specified position.
    /// </summary>
    /// <param name="_cellPosition">The position of the cell to highlight.</param>
    public void HighlightCell(Vector3Int _cellPosition)
    {
        Vector3 start = new Vector3(_cellPosition.x, _cellPosition.y, _cellPosition.z) * cellSize - gridOffset;
        Vector3 end = start + Vector3.one * cellSize;

        Vector3[] positions = new Vector3[]
        {
            start,
            new(end.x, start.y, start.z),
            new(start.x, end.y, start.z),
            new(start.x, start.y, end.z),
            new(end.x, end.y, start.z),
            new(end.x, start.y, end.z),
            new(start.x, end.y, end.z),
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

    /// <summary>
    /// Unhighlights the currently highlighted cell.
    /// </summary>
    public void UnhighlightCell()
    {
        foreach (var lr in highlightLines)
        {
            lr.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Shows the next layer of the grid.
    /// </summary>
    public void ShowLayer()
    {
        if (VisibleLayers < gridSize + 1)
        {
            VisibleLayers++;
            UpdateVisibleLayers();
        }
    }

    /// <summary>
    /// Hides the top layer of the grid.
    /// </summary>
    public void HideLayer()
    {
        if (VisibleLayers > 2)
        {
            VisibleLayers--;
            UpdateVisibleLayers();
        }
    }

    /// <summary>
    /// Creates the grid lines for the visual grid.
    /// </summary>
    private void CreateGrid()
    {
        ClearExistingLines(gridLines);
        layerLines.Clear();
        verticalLines.Clear();

        gridOffset = new Vector3(gridSize / 2f, gridSize / 2f, gridSize / 2f) * cellSize;

        for (int y = 0; y <= gridSize; y++)
        {
            List<LineRenderer> currentLayerLines = new();

            for (int i = 0; i <= gridSize; i++)
            {
                float pos = i * cellSize;

                // X direction
                currentLayerLines.Add(CreateGridLine(new Vector3(0, y * cellSize, pos) - gridOffset, new Vector3(gridSize * cellSize, y * cellSize, pos) - gridOffset));

                // Y direction
                verticalLines.Add(CreateGridLine(new Vector3(y * cellSize, 0, i * cellSize) - gridOffset, new Vector3(y * cellSize, gridSize * cellSize, i * cellSize) - gridOffset));

                // Z direction
                currentLayerLines.Add(CreateGridLine(new Vector3(pos, y * cellSize, 0) - gridOffset, new Vector3(pos, y * cellSize, gridSize * cellSize) - gridOffset));
            }

            layerLines.Add(currentLayerLines);
        }

        VisibleLayers = gridSize + 1;
        UpdateVisibleLayers();
    }

    /// <summary>
    /// Creates the highlight lines for the visual grid.
    /// </summary>
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

    /// <summary>
    /// Creates a grid line for the visual grid.
    /// </summary>
    /// <param name="_start">The start position of the line.</param>
    /// <param name="_end">The end position of the line.</param>
    /// <returns>The created LineRenderer component.</returns>
    private LineRenderer CreateGridLine(Vector3 _start, Vector3 _end)
    {
        return CreateLine(_start, _end, gridColor, gridLineWidth, gridLines);
    }

    /// <summary>
    /// Creates a line for the visual grid.
    /// </summary>
    /// <param name="_start">The start position of the line.</param>
    /// <param name="_end">The end position of the line.</param>
    /// <param name="_color">The color of the line.</param>
    /// <param name="_width">The width of the line.</param>
    /// <param name="_lineList">The list to add the LineRenderer component to.</param>
    /// <returns>The created LineRenderer component.</returns>
    private LineRenderer CreateLine(Vector3 _start, Vector3 _end, Color _color, float _width, List<LineRenderer> _lineList = null)
    {
        GameObject lineObj = new("GridLine");
        lineObj.transform.SetParent(transform);
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

    /// <summary>
    /// Clears the existing grid lines.
    /// </summary>
    /// <param name="_lines">The list of LineRenderer components to clear.</param>
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

    /// <summary>
    /// Updates the visibility of the grid layers based on the VisibleLayers property.
    /// </summary>
    private void UpdateVisibleLayers()
    {
        for (int y = 0; y < layerLines.Count; y++)
        {
            bool isVisible = y < VisibleLayers;

            foreach (var line in layerLines[y])
            {
                line.gameObject.SetActive(isVisible);
            }
        }

        float visibleHeight = (VisibleLayers - 1) * cellSize;

        foreach (var line in verticalLines)
        {
            Vector3 startPos = line.GetPosition(0);
            Vector3 endPos = line.GetPosition(1);

            endPos.y = gridSize * cellSize - gridOffset.y;
            endPos.y = Mathf.Min(endPos.y, visibleHeight - gridOffset.y);

            line.SetPosition(0, startPos);
            line.SetPosition(1, endPos);
        }
    }
}
