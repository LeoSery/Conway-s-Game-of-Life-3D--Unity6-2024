using System.Collections.Generic;

using UnityEngine;

using Unity.Mathematics;

public class GameManager : MonoBehaviour
{
    private Grid grid;
    public float updateInterval = 1f;
    private float lastUpdateTime;

    public GameObject cellPrefab;
    private Dictionary<int3, GameObject> cellObjects;

    private void Start()
    {
        grid = new Grid();
        cellObjects = new Dictionary<int3, GameObject>();
        InitializeGrid();
    }

    private void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateGrid();
            lastUpdateTime = Time.time;
        }
    }

    private void InitializeGrid()
    {
        // Example: Create a simple pattern
        grid.SetAlive(new int3(0, 0, 0));
        grid.SetAlive(new int3(1, 0, 0));
        grid.SetAlive(new int3(0, 1, 0));
        grid.SetAlive(new int3(1, 1, 0));
        grid.SetAlive(new int3(0, 0, 1));

        // Render initial cells
        foreach (var cell in grid.GetActiveCells())
        {
            if (cell.State == 1) // Only render living cells
            {
                CreateCellObject(cell.Position);
            }
        }
    }

    private void UpdateGrid()
    {
        var newStates = new Dictionary<int3, byte>();

        foreach (var cell in grid.GetActiveCells())
        {
            int aliveNeighbors = grid.CountAliveNeighbors(cell.Position);
            newStates[cell.Position] = DetermineNewState(cell.State, aliveNeighbors);
        }

        // Apply new states and update rendering
        foreach (var kvp in newStates)
        {
            if (kvp.Value == 1)
            {
                grid.SetAlive(kvp.Key);
                if (!cellObjects.ContainsKey(kvp.Key))
                {
                    CreateCellObject(kvp.Key);
                }
            }
            else
            {
                grid.RemoveCell(kvp.Key);
                if (cellObjects.ContainsKey(kvp.Key))
                {
                    DestroyCellObject(kvp.Key);
                }
            }
        }
    }

    private byte DetermineNewState(byte currentState, int aliveNeighbors)
    {
        if (currentState == 1) // ALIVE
        {
            return (byte)((aliveNeighbors == 2 || aliveNeighbors == 3) ? 1 : 0);
        }
        else
        {
            return (byte)(aliveNeighbors == 3 ? 1 : 0);
        }
    }

    private void CreateCellObject(int3 position)
    {
        GameObject cellObject = Instantiate(cellPrefab, new Vector3(position.x, position.y, position.z), Quaternion.identity);
        cellObjects[position] = cellObject;
    }

    private void DestroyCellObject(int3 position)
    {
        if (cellObjects.TryGetValue(position, out GameObject cellObject))
        {
            Destroy(cellObject);
            cellObjects.Remove(position);
        }
    }
}
