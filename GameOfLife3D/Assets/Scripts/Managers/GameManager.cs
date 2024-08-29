using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Unity.Mathematics;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public delegate void CycleCompleteHandler();
    public static event CycleCompleteHandler OnCycleComplete;

    public delegate void PauseStateChangedHandler(bool isPaused);
    public static event PauseStateChangedHandler OnPauseStateChanged;

    [Header("Simulation Settings :")]
    [SerializeField, Range(0.1f, 10f)]
    private float updateInterval = 1f;
    public int gridSize = 10;

    [Header("Simulation Limits :")]
    public int minGridSize = 5;
    public int maxGridSize = 50;
    [Space(10)]
    public float minCycleSpeed = 0.1f;
    public float maxCycleSpeed = 10f;

    public float UpdateInterval
    {
        get => updateInterval;
        set
        {
            updateInterval = Mathf.Clamp(value, minCycleSpeed, maxCycleSpeed);
        }
    }

    [Header("Prefabs :")]
    public GameObject cellPrefab;
    public Transform cellContainer;
    public VisualGrid visualGrid;

    public Grid Grid { get; private set; }
    public bool IsPaused => isPaused;

    private Dictionary<int3, GameObject> cellObjects;
    private float lastUpdateTime;
    private int cellSize = 1;
    private bool isPaused = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeGrid();

        if (visualGrid != null)
        {
            visualGrid.Initialize(gridSize, cellSize);
        }
    }

    private void Update()
    {
        if (!isPaused && Time.time - lastUpdateTime >= UpdateInterval)
        {
            UpdateGrid();
            lastUpdateTime = Time.time;
        }
    }

    private void InitializeGrid()
    {
        Grid = new Grid(gridSize);
        cellObjects = new Dictionary<int3, GameObject>();

        // Example: Create a simple pattern
        Grid.SetAlive(new int3(4, 4, 4));
        Grid.SetAlive(new int3(4, 4, 5));
        Grid.SetAlive(new int3(4, 5, 4));
        Grid.SetAlive(new int3(5, 4, 4));
        Grid.SetAlive(new int3(5, 5, 5));
        Grid.SetAlive(new int3(3, 4, 4));
        Grid.SetAlive(new int3(4, 3, 4));

        // Render initial cells
        foreach (var cell in Grid.GetActiveCells())
        {
            if (cell.State == CellState.Alive)
            {
                CreateCellObject(cell.Position);
            }
        }
        StatManager.Instance.InitializeStats();
    }

    public void ResizeGrid(int newSize)
    {
        gridSize = newSize;
        Grid.Resize(newSize);

        if (visualGrid != null)
        {
            visualGrid.UpdateGridSize(gridSize, cellSize);
        }
        ResetGrid();
        StatManager.Instance.UpdateTotalCells();
    }

    private void UpdateGrid()
    {
        var newStates = new Dictionary<int3, byte>();
        int aliveBefore = Grid.GetActiveCells().Count(c => c.State == CellState.Alive);

        foreach (var cell in Grid.GetActiveCells())
        {
            if (cell.State == CellState.Alive || cell.State == CellState.ActiveZone)
            {
                int aliveNeighbors = Grid.CountAliveNeighbors(cell.Position);
                byte newState = DetermineNewState(cell.State, aliveNeighbors);
                newStates[cell.Position] = newState;
            }
        }

        // Apply new states and update rendering
        foreach (var kvp in newStates)
        {
            if (kvp.Value == CellState.Alive)
            {
                Grid.SetAlive(kvp.Key);

                if (!cellObjects.ContainsKey(kvp.Key))
                {
                    CreateCellObject(kvp.Key);
                }
            }
            else
            {
                Grid.RemoveCell(kvp.Key);

                if (cellObjects.ContainsKey(kvp.Key))
                {
                    DestroyCellObject(kvp.Key);
                }
            }
        }
        OnCycleComplete?.Invoke();
    }

    private byte DetermineNewState(byte currentState, int aliveNeighbors)
    {
        if (currentState == CellState.Alive)
        {
            // A living cell survives if it has 4, 5 or 6 living neighbors
            return (byte)((aliveNeighbors >= 4 && aliveNeighbors <= 6) ? CellState.Alive : CellState.Dead);
        }
        else
        {
            // A dead cell is born if it has exactly 4 living neighbors.
            return (byte)(aliveNeighbors == 4 ? CellState.Alive : CellState.Dead);
        }
    }

    private void CreateCellObject(int3 position)
    {
        Vector3 worldPosition = new Vector3(
            position.x - (gridSize - 1) / 2f,
            position.y - (gridSize - 1) / 2f,
            position.z - (gridSize - 1) / 2f
        ) * cellSize;

        GameObject cellObject = Instantiate(cellPrefab, worldPosition, Quaternion.identity, cellContainer);
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

    public void TogglePause()
    {
        isPaused = !isPaused;
        StatManager.Instance.SetPaused(isPaused);
        OnPauseStateChanged?.Invoke(isPaused);
        Debug.Log(isPaused ? "Game Paused" : "Game Resumed");
    }

    public void ResetGrid()
    {
        // Destroy all existing cell objects
        foreach (var cellObject in cellObjects.Values)
        {
            Destroy(cellObject);
        }

        cellObjects.Clear();

        // Reset the grid
        Grid = new Grid(gridSize);
        InitializeGrid();

        StatManager.Instance.ResetStats();
        StatManager.Instance.SetPaused(true);
        Debug.Log("Grid and Stats Reset");
    }

    public void IncreaseSpeed()
    {
        UpdateInterval -= 0.1f;
        Debug.Log($"Speed Increased. New interval: {UpdateInterval}");
    }

    public void DecreaseSpeed()
    {
        UpdateInterval += 0.1f;
        Debug.Log($"Speed Decreased. New interval: {UpdateInterval}");
    }
}
