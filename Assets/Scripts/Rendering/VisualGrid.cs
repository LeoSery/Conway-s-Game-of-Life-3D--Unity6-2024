using System.Collections.Generic;

using UnityEngine;

public class VisualGrid : MonoBehaviour
{
    [Header("Settings :")]
    public Color gridColor = new(0.5f, 0.5f, 0.5f, 0.2f);
    public Color boundingBoxColor = new(1f, 0f, 0f, 0.8f); // Couleur du cube extérieur
    public Color highlightColor = Color.yellow;
    public float gridLineWidth = 0.02f;
    public float boundingBoxLineWidth = 0.03f; // Épaisseur des lignes du cube extérieur
    public float highlightLineWidth = 0.04f;

    private int gridSize;
    private float cellSize;
    private Vector3 gridOffset;

    private readonly List<LineRenderer> gridLines = new();
    private readonly List<LineRenderer> highlightLines = new();
    private readonly List<LineRenderer> boundingBoxLines = new();
    private readonly List<List<LineRenderer>> layerGrids = new();
    private int currentVisibleLayer = 0;

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
        {0, 1}, {1, 2}, {2, 3}, {3, 0}, // Bord inférieur
        {4, 5}, {5, 6}, {6, 7}, {7, 4}, // Bord supérieur
        {0, 4}, {1, 5}, {2, 6}, {3, 7}  // Lignes verticales connectant les bords
    };

    /// <summary>
    /// L'index de la couche actuellement visible.
    /// </summary>
    public int CurrentVisibleLayer => currentVisibleLayer;

    /// <summary>
    /// Le nombre total de couches dans la grille.
    /// </summary>
    public int TotalLayers => gridSize;

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
            new Vector3(start.x + cellSize, start.y, start.z),
            new Vector3(start.x + cellSize, start.y, start.z + cellSize),
            new Vector3(start.x, start.y, start.z + cellSize),
            new Vector3(start.x, start.y + cellSize, start.z),
            new Vector3(start.x + cellSize, start.y + cellSize, start.z),
            new Vector3(start.x + cellSize, start.y + cellSize, start.z + cellSize),
            new Vector3(start.x, start.y + cellSize, start.z + cellSize)
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
        if (currentVisibleLayer < gridSize - 1)
        {
            currentVisibleLayer++;
            UpdateVisibleLayers();
        }
    }

    /// <summary>
    /// Hides the top layer of the grid.
    /// </summary>
    public void HideLayer()
    {
        if (currentVisibleLayer > 0)
        {
            currentVisibleLayer--;
            UpdateVisibleLayers();
        }
    }

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
        // Nettoyer les lignes existantes
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

        // Créer une grille de cubes pour chaque couche
        for (int y = 0; y < gridSize; y++)
        {
            List<LineRenderer> layerLines = new();
            float yPos = y * cellSize;

            // Pour chaque cellule dans cette couche
            for (int z = 0; z < gridSize; z++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    // Créer un cube pour chaque cellule
                    Vector3 cellStart = new Vector3(x * cellSize, yPos, z * cellSize) - gridOffset;

                    // Les 8 sommets du cube de la cellule
                    Vector3[] corners = new Vector3[]
                    {
                        cellStart,
                        new Vector3(cellStart.x + cellSize, cellStart.y, cellStart.z),
                        new Vector3(cellStart.x + cellSize, cellStart.y, cellStart.z + cellSize),
                        new Vector3(cellStart.x, cellStart.y, cellStart.z + cellSize),
                        new Vector3(cellStart.x, cellStart.y + cellSize, cellStart.z),
                        new Vector3(cellStart.x + cellSize, cellStart.y + cellSize, cellStart.z),
                        new Vector3(cellStart.x + cellSize, cellStart.y + cellSize, cellStart.z + cellSize),
                        new Vector3(cellStart.x, cellStart.y + cellSize, cellStart.z + cellSize)
                    };

                    // Dessiner les 12 arêtes du cube
                    for (int i = 0; i < 12; i++)
                    {
                        LineRenderer lr = CreateLine(
                            corners[CubeEdges[i, 0]],
                            corners[CubeEdges[i, 1]],
                            gridColor,
                            gridLineWidth
                        );
                        lr.gameObject.SetActive(false); // Désactivé par défaut
                        layerLines.Add(lr);
                    }
                }
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
