using System.Collections.Generic;

using UnityEngine;

public class VisualGrid : MonoBehaviour
{
    [Header("Settings :")]
    public Color gridColor = new(0.5f, 0.5f, 0.5f, 0.2f);
    public Color boundingBoxColor = new(1f, 0f, 0f, 0.8f);
    public Color highlightColor = Color.yellow;

    public float gridLineWidth = 0.02f;
    public float boundingBoxLineWidth = 0.03f;
    public float highlightLineWidth = 0.04f;

    public bool HideGridOnSimulate = false;
    public bool isSiulationRunning = false;

    private int currentVisibleLayer = 0;
    private float cellSize;
    private int gridSize;

    private Vector3 gridOffset;

    private readonly List<LineRenderer> gridLines = new();
    private readonly List<LineRenderer> highlightLines = new();
    private readonly List<LineRenderer> boundingBoxLines = new();
    private readonly List<List<LineRenderer>> layerGrids = new();

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
        {0, 1}, {1, 2}, {2, 3}, {3, 0}, // Bottom edge
        {4, 5}, {5, 6}, {6, 7}, {7, 4}, // Top edge
        {0, 4}, {1, 5}, {2, 6}, {3, 7}  // Vertical lines connecting edges
    };

    public int CurrentVisibleLayer => currentVisibleLayer;
    public int VisibleLayers => gridSize;


    /// <summary>
    /// Subscribes to the OnPauseStateChanged event when the object is enabled.
    /// </summary>
    private void OnEnable()
    {
        GameManager.OnPauseStateChanged += HandlePauseStateChanged;
    }

    /// <summary>
    /// Unsubscribes from the OnPauseStateChanged event when the object is disabled.
    /// </summary>
    private void OnDisable()
    {
        GameManager.OnPauseStateChanged -= HandlePauseStateChanged;
    }

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
        CreateBoundingBox();
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
        currentVisibleLayer = 0;
        CreateGrid();
        CreateBoundingBox();
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
            new(start.x + cellSize, start.y, start.z),
            new(start.x + cellSize, start.y, start.z + cellSize),
            new(start.x, start.y, start.z + cellSize),
            new(start.x, start.y + cellSize, start.z),
            new(start.x + cellSize, start.y + cellSize, start.z),
            new(start.x + cellSize, start.y + cellSize, start.z + cellSize),
            new(start.x, start.y + cellSize, start.z + cellSize)
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
        if (!isSiulationRunning || !HideGridOnSimulate)
        {
            if (currentVisibleLayer < gridSize - 1)
            {
                currentVisibleLayer++;
                UpdateVisibleLayers();
            }
        }
    }

    /// <summary>
    /// Hides the top layer of the grid.
    /// </summary>
    public void HideLayer()
    {
        if (!isSiulationRunning || !HideGridOnSimulate)
        {
            if (currentVisibleLayer > 0)
            {
                currentVisibleLayer--;
                UpdateVisibleLayers();
            }
        }
    }

    /// <summary>
    /// Creates the bounding box for the visual grid.
    /// </summary>
    /// <remarks>
    /// This method generates the 12 edges of a cube that represents the bounding box
    /// of the grid. The edges are created using LineRenderer components and are stored
    /// in the boundingBoxLines list.
    /// </remarks>
    private void CreateBoundingBox()
    {
        ClearExistingLines(boundingBoxLines);

        Vector3 min = Vector3.zero - gridOffset;
        Vector3 max = new Vector3(gridSize, gridSize, gridSize) * cellSize - gridOffset;

        // Les 8 sommets du cube
        Vector3[] corners = new Vector3[]
        {
            new(min.x, min.y, min.z),
            new(max.x, min.y, min.z),
            new(max.x, min.y, max.z),
            new(min.x, min.y, max.z),
            new(min.x, max.y, min.z),
            new(max.x, max.y, min.z),
            new(max.x, max.y, max.z),
            new(min.x, max.y, max.z)
        };

        // Créer les 12 arêtes du cube
        for (int i = 0; i < 12; i++)
        {
            LineRenderer lr = CreateLine(
                corners[CubeEdges[i, 0]],
                corners[CubeEdges[i, 1]],
                boundingBoxColor,
                boundingBoxLineWidth,
                boundingBoxLines
            );
        }
    }

    /// <summary>
    /// Creates the grid lines for the visual grid.
    /// </summary>
    private void CreateGrid()
    {
        // Clean up existing lines
        foreach (var layer in layerGrids)
        {
            foreach (var line in layer)
            {
                if (line != null)
                {
                    Destroy(line.gameObject);
                }
            }
        }
        layerGrids.Clear();

        gridOffset = new Vector3(gridSize / 2f, gridSize / 2f, gridSize / 2f) * cellSize;

        for (int y = 0; y < gridSize; y++)
        {
            List<LineRenderer> layerLines = new();
            float yPos = y * cellSize;
            float yPosTop = (y + 1) * cellSize;

            for (int z = 0; z <= gridSize; z++)
            {
                float zPos = z * cellSize;
                LineRenderer lineX = CreateLine(new Vector3(0, yPos, zPos) - gridOffset, new Vector3(gridSize * cellSize, yPos, zPos) - gridOffset, gridColor, gridLineWidth);
                layerLines.Add(lineX);

                LineRenderer lineXTop = CreateLine(new Vector3(0, yPosTop, zPos) - gridOffset, new Vector3(gridSize * cellSize, yPosTop, zPos) - gridOffset, gridColor, gridLineWidth);
                layerLines.Add(lineXTop);
            }

            for (int x = 0; x <= gridSize; x++)
            {
                float xPos = x * cellSize;
                LineRenderer lineZ = CreateLine(new Vector3(xPos, yPos, 0) - gridOffset, new Vector3(xPos, yPos, gridSize * cellSize) - gridOffset, gridColor, gridLineWidth);
                layerLines.Add(lineZ);

                LineRenderer lineZTop = CreateLine(new Vector3(xPos, yPosTop, 0) - gridOffset, new Vector3(xPos, yPos + cellSize, gridSize * cellSize) - gridOffset, gridColor, gridLineWidth );
                layerLines.Add(lineZTop);
            }

            for (int z = 0; z <= gridSize; z++)
            {
                for (int x = 0; x <= gridSize; x++)
                {
                    float xPos = x * cellSize;
                    float zPos = z * cellSize;
                    LineRenderer lineY = CreateLine(new Vector3(xPos, yPos, zPos) - gridOffset, new Vector3(xPos, yPosTop, zPos) - gridOffset, gridColor, gridLineWidth);
                    layerLines.Add(lineY);
                }
            }

            foreach (var line in layerLines)
            {
                line.gameObject.SetActive(false);
            }

            layerGrids.Add(layerLines);
        }
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
    /// Handles the pause state change event.
    /// </summary>
    /// <param name="isPaused">Indicates whether the game is paused.</param>
    /// <remarks>
    /// This method updates the local simulation running state and shows or hides the grid lines
    /// based on the pause state and the HideGridOnSimulate setting.
    /// </remarks>
    private void HandlePauseStateChanged(bool isPaused)
    {
        isSiulationRunning = !isPaused;

        if (HideGridOnSimulate)
        {
            if (isPaused)
            {
                ShowGridLines(true);
            }
            else
            {
                ShowGridLines(false);
            }
        }

        GameManager.Instance.cellInteractionController.UpdateCellHighlight();
    }

    /// <summary>
    /// Shows or hides the grid lines based on the specified parameter.
    /// </summary>
    /// <param name="show">If true, the grid lines will be shown; otherwise, they will be hidden.</param>
    /// <remarks>
    /// This method updates the visibility of the grid lines. If the grid has no layers, the method returns immediately.
    /// When showing the grid lines, it calls <see cref="UpdateVisibleLayers"/> to update the visibility of the layers.
    /// When hiding the grid lines, it iterates through all layers and deactivates each line.
    /// </remarks>
    public void ShowGridLines(bool show)
    {
        if (layerGrids.Count <= 0) return;

        if (show)
        {
            UpdateVisibleLayers();
        }
        else
        {
            for (int y = 0; y < layerGrids.Count; y++)
            {
                foreach (var line in layerGrids[y])
                {
                    line.gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Sets the visibility of the grid when the game is playing.
    /// </summary>
    /// <param name="value">If true, the grid will be hidden when the game is playing; otherwise, it will be shown.</param>
    /// <remarks>
    /// This method updates the HideGridOnSimulate property and calls HandlePauseStateChanged
    /// to update the grid visibility based on the current pause state.
    /// </remarks>
    public void SetHideGridWhenPlaying(bool value)
    {
        HideGridOnSimulate = value;

        if (GameManager.Instance != null)
        {
            HandlePauseStateChanged(GameManager.Instance.IsPaused);
        }
    }

    /// <summary>
    /// Updates the visibility of the grid layers based on the VisibleLayers property.
    /// </summary>
    private void UpdateVisibleLayers()
    {
        for (int y = 0; y < layerGrids.Count; y++)
        {
            bool isVisible = y == currentVisibleLayer;
            foreach (var line in layerGrids[y])
            {
                line.gameObject.SetActive(isVisible);
            }
        }
    }
}
