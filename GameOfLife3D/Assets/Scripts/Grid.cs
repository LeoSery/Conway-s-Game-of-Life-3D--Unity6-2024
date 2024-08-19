using System.Collections.Generic;
using System.Linq;

using Unity.Mathematics;

public class Grid
{
    private HashSet<int3> activeCells;
    private Dictionary<int3, byte> cellStates;

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

    public Grid()
    {
        activeCells = new HashSet<int3>();
        cellStates = new Dictionary<int3, byte>();
    }

    public void SetAlive(int3 position)
    {
        if (!cellStates.ContainsKey(position) || cellStates[position] != 1)
        {
            activeCells.Add(position);
            cellStates[position] = 1;
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

            if (!cellStates.ContainsKey(neighbor))
            {
                cellStates[neighbor] = 2;
            }
        }
    }

    private void UpdateInactiveArea(int3 position)
    {
        foreach (var offset in neighborOffsets)
        {
            int3 neighbor = position + offset;

            if (cellStates.TryGetValue(neighbor, out byte state) && state == 2)
            {
                bool hasLivingNeighbor = false;

                foreach (var innerOffset in neighborOffsets)
                {
                    int3 innerNeighbor = neighbor + innerOffset;

                    if (cellStates.TryGetValue(innerNeighbor, out byte innerState) && innerState == 1)
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
        return cellStates.TryGetValue(position, out byte state) && state == 1;
    }

    public int CountAliveNeighbors(int3 position)
    {
        return neighborOffsets.Count(offset => IsAlive(position + offset));
    }

    public IEnumerable<Cell> GetActiveCells()
    {
        return cellStates.Select(kvp => new Cell(kvp.Key, kvp.Value));
    }
}
