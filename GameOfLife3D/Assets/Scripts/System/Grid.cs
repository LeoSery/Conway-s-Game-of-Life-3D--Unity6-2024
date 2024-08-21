using System.Collections.Generic;
using System.Linq;

using Unity.Mathematics;

public class Grid
{
    private readonly HashSet<int3> activeCells;
    private readonly Dictionary<int3, byte> cellStates;

    private int gridSize;
    public int GridSize => gridSize;

    private static readonly int3[] neighborOffsets =
    {
        new int3(-1, -1, -1), new int3(-1, -1, 0), new int3(-1, -1, 1),
        new int3(-1, 0, -1),  new int3(-1, 0, 0),  new int3(-1, 0, 1),
        new int3(-1, 1, -1),  new int3(-1, 1, 0),  new int3(-1, 1, 1),
        new int3(0, -1, -1),  new int3(0, -1, 0),  new int3(0, -1, 1),
        new int3(0, 0, -1),                        new int3(0, 0, 1),
        new int3(0, 1, -1),   new int3(0, 1, 0),   new int3(0, 1, 1),
        new int3(1, -1, -1),  new int3(1, -1, 0),  new int3(1, -1, 1),
        new int3(1, 0, -1),   new int3(1, 0, 0),   new int3(1, 0, 1),
        new int3(1, 1, -1),   new int3(1, 1, 0),   new int3(1, 1, 1)
    };

    public Grid(int size)
    {
        gridSize = size;
        activeCells = new HashSet<int3>();
        cellStates = new Dictionary<int3, byte>();
    }

    public void SetAlive(int3 position)
    {
        if (IsWithinBounds(position) && (!cellStates.ContainsKey(position) || cellStates[position] != 1))
        {
            activeCells.Add(position);
            cellStates[position] = CellState.Alive;
            UpdateActiveArea(position);
        }
    }

    public void RemoveCell(int3 position)
    {
        if (cellStates.ContainsKey(position))
        {
            activeCells.Remove(position);
            cellStates.Remove(position);
            UpdateInactiveArea(position);
        }
    }

    private void UpdateActiveArea(int3 position)
    {
        foreach (var offset in neighborOffsets)
        {
            int3 neighbor = position + offset;

            if (IsWithinBounds(neighbor) && !cellStates.ContainsKey(neighbor))
            {
                cellStates[neighbor] = CellState.ActiveZone;
            }
        }
    }

    private void UpdateInactiveArea(int3 position)
    {
        foreach (var offset in neighborOffsets)
        {
            int3 neighbor = position + offset;

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

    public bool IsAlive(int3 position)
    {
        return IsWithinBounds(position) && cellStates.TryGetValue(position, out byte state) && state == CellState.Alive;
    }

    public int CountAliveNeighbors(int3 position)
    {
        return neighborOffsets.Count(offset =>
        {
            int3 neighborPos = position + offset;
            return cellStates.TryGetValue(neighborPos, out byte state) && state == 1;
        });
    }

    public IEnumerable<Cell> GetActiveCells()
    {
        return cellStates.Select(kvp => new Cell(kvp.Key, kvp.Value));
    }

    private bool IsWithinBounds(int3 position)
    {
        return position.x >= 0 && position.x < gridSize &&
               position.y >= 0 && position.y < gridSize &&
               position.z >= 0 && position.z < gridSize;
    }

    public void Resize(int newSize)
    {
        gridSize = newSize;

        // Remove cells that are now out of bounds
        var cellsToRemove = cellStates.Keys.Where(pos => !IsWithinBounds(pos)).ToList();

        foreach (var pos in cellsToRemove)
        {
            RemoveCell(pos);
        }
    }
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
}
