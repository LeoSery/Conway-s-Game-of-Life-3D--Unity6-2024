using System.Collections.Generic;
using System.Linq;

using Unity.Mathematics;

/// <summary>
/// Represents a grid of cells.
/// </summary>
public class Grid
{
    #region Private Fields
    private readonly HashSet<int3> activeCells;
    private readonly Dictionary<int3, byte> cellStates;
    private int gridSize;

    private static readonly int3[] neighborOffsets =
    {
        new(-1, -1, -1), new(-1, -1, 0), new(-1, -1, 1),
        new(-1, 0, -1),  new(-1, 0, 0),  new(-1, 0, 1),
        new(-1, 1, -1),  new(-1, 1, 0),  new(-1, 1, 1),
        new(0, -1, -1),  new(0, -1, 0),  new(0, -1, 1),
        new(0, 0, -1),                   new(0, 0, 1),
        new(0, 1, -1),   new(0, 1, 0),   new(0, 1, 1),
        new(1, -1, -1),  new(1, -1, 0),  new(1, -1, 1),
        new(1, 0, -1),   new(1, 0, 0),   new(1, 0, 1),
        new(1, 1, -1),   new(1, 1, 0),   new(1, 1, 1)
    };
    #endregion

    #region Properties
    /// <summary>
    /// Gets the size of the grid.
    /// </summary>
    public int GridSize => gridSize;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Grid"/> class with the specified size.
    /// </summary>
    /// <param name="_size">The size of the grid.</param>
    public Grid(int _size)
    {
        gridSize = _size;
        activeCells = new HashSet<int3>();
        cellStates = new Dictionary<int3, byte>();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Sets the cell at the specified position to be alive.
    /// </summary>
    /// <param name="_position">The position of the cell.</param>
    public void SetAlive(int3 _position)
    {
        if (IsWithinBounds(_position) && (!cellStates.ContainsKey(_position) || cellStates[_position] != 1))
        {
            activeCells.Add(_position);
            cellStates[_position] = CellState.Alive;
            UpdateActiveArea(_position);
        }
    }

    /// <summary>
    /// Removes the cell at the specified position.
    /// </summary>
    /// <param name="_position">The position of the cell.</param>
    public void RemoveCell(int3 _position)
    {
        if (cellStates.ContainsKey(_position))
        {
            activeCells.Remove(_position);
            cellStates.Remove(_position);
            UpdateInactiveArea(_position);
        }
    }

    /// <summary>
    /// Determines whether the cell at the specified position is alive.
    /// </summary>
    /// <param name="_position">The position of the cell.</param>
    /// <returns><c>true</c> if the cell is alive; otherwise, <c>false</c>.</returns>
    public bool IsAlive(int3 _position)
    {
        return IsWithinBounds(_position) && cellStates.TryGetValue(_position, out byte state) && state == CellState.Alive;
    }

    /// <summary>
    /// Counts the number of alive neighbors for the cell at the specified position.
    /// </summary>
    /// <param name="_position">The position of the cell.</param>
    /// <returns>The number of alive neighbors.</returns>
    public int CountAliveNeighbors(int3 _position)
    {
        return neighborOffsets.Count(offset =>
        {
            int3 neighborPos = _position + offset;
            return cellStates.TryGetValue(neighborPos, out byte state) && state == 1;
        });
    }

    /// <summary>
    /// Gets all the active cells in the grid.
    /// </summary>
    /// <returns>An enumerable collection of active cells.</returns>
    public IEnumerable<Cell> GetActiveCells()
    {
        return cellStates.Select(kvp => new Cell(kvp.Key, kvp.Value));
    }

    /// <summary>
    /// Resizes the grid to the specified size.
    /// </summary>
    /// <param name="_newSize">The new size of the grid.</param>
    public void Resize(int _newSize)
    {
        gridSize = _newSize;

        var cellsToRemove = cellStates.Keys.Where(pos => !IsWithinBounds(pos)).ToList();

        foreach (var pos in cellsToRemove)
        {
            RemoveCell(pos);
        }
    }

    /// <summary>
    /// Gets the name of the state from its value.
    /// </summary>
    /// <param name="_state">The value of the state.</param>
    /// <returns>The name of the state.</returns>
    public string GetStateNameFromValue(byte _state)
    {
        return _state switch
        {
            CellState.Dead => "Dead",
            CellState.Alive => "Alive",
            CellState.ActiveZone => "Active Zone",
            _ => "Unknown",
        };
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Updates the active area around the specified position.
    /// </summary>
    /// <param name="_position">The position to update.</param>
    private void UpdateActiveArea(int3 _position)
    {
        foreach (var offset in neighborOffsets)
        {
            int3 neighbor = _position + offset;

            if (IsWithinBounds(neighbor) && !cellStates.ContainsKey(neighbor))
            {
                cellStates[neighbor] = CellState.ActiveZone;
            }
        }
    }

    /// <summary>
    /// Updates the inactive area around the specified position.
    /// </summary>
    /// <param name="_position">The position to update.</param>
    private void UpdateInactiveArea(int3 _position)
    {
        foreach (var offset in neighborOffsets)
        {
            int3 neighbor = _position + offset;

            if (IsWithinBounds(neighbor) && cellStates.TryGetValue(neighbor, out byte state) && state == 2)
            {
                bool hasLivingNeighbor = false;

                foreach (var innerOffset in neighborOffsets)
                {
                    int3 innerNeighbor = neighbor + innerOffset;

                    if (IsWithinBounds(innerNeighbor) && cellStates.TryGetValue(innerNeighbor, out byte innerState) && innerState == 1)
                    {
                        hasLivingNeighbor = true;
                        break;
                    }
                }

                if (!hasLivingNeighbor)
                {
                    cellStates.Remove(neighbor);
                }
            }
        }
    }

    /// <summary>
    /// Checks if the specified position is within the bounds of the grid.
    /// </summary>
    /// <param name="_position">The position to check.</param>
    /// <returns><c>true</c> if the position is within the bounds of the grid; otherwise, <c>false</c>.</returns>
    private bool IsWithinBounds(int3 _position)
    {
        return _position.x >= 0 && _position.x < gridSize &&
               _position.y >= 0 && _position.y < gridSize &&
               _position.z >= 0 && _position.z < gridSize;
    }
    #endregion
}
