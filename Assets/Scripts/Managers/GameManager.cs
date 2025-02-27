using System.Collections.Generic;
using System.Linq;

using Unity.Mathematics;

using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Static Members and Events
    public static GameManager Instance { get; private set; }

    public delegate void CycleCompleteHandler();
    public static event CycleCompleteHandler OnCycleComplete;

    public delegate void PauseStateChangedHandler(bool _isPaused);
    public static event PauseStateChangedHandler OnPauseStateChanged;
    #endregion

    #region Fields and Properties
    [Header("Simulation Settings")]
    [SerializeField] private float updateInterval = 1f;
    public int gridSize = 10;

    [Header("Simulation Limits")]
    public int minGridSize = 5;
    public int maxGridSize = 50;
    public float minCycleSpeed = 0.1f;
    public float maxCycleSpeed = 10f;

    [Header("Prefabs")]
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Transform cellContainer;

    [Header("Scripts")]
    public VisualGrid visualGrid;
    public CellInteractionController cellInteractionController;
    public CameraController CameraController;
    public CellPool cellPool;

    private Dictionary<int3, GameObject> cellObjects;
    private float lastUpdateTime;
    private const int CELL_SIZE = 1;
    public int CellSize => CELL_SIZE;
    private bool isPaused = true;

    public float UpdateInterval
    {
        get => updateInterval;
        set => updateInterval = Mathf.Clamp(value, minCycleSpeed, maxCycleSpeed);
    }
    public Grid Grid { get; private set; }
    public bool IsPaused => isPaused;
    #endregion

    #region Unity Lifecycle Methods
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CalculateAndSetPoolSize(gridSize);
            InitializeGrid();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (visualGrid != null)
        {
            visualGrid.Initialize(gridSize, CELL_SIZE);
        }

        InitializeStats();
    }

    private void Update()
    {
        if (!isPaused)
        {
            float timeSinceLastUpdate = Time.time - lastUpdateTime;
            if (timeSinceLastUpdate >= UpdateInterval)
            {
                Debug.Log($"Cycle Update - Interval: {UpdateInterval}, Time since last: {timeSinceLastUpdate}");
                UpdateGrid();
                lastUpdateTime = Time.time;
            }
        }
    }
    #endregion

    #region Public Methods
    public void ResizeGrid(int _newSize)
    {
        gridSize = _newSize;
        Grid.Resize(_newSize);
        CalculateAndSetPoolSize(_newSize);

        if (visualGrid != null)
        {
            visualGrid.UpdateGridSize(gridSize, CELL_SIZE);
        }

        ResetGrid();
        StatManager.Instance.UpdateTotalCells();
    }

    public void CreateCell(int3 _position)
    {
        if (!Grid.IsAlive(_position))
        {
            Grid.SetAlive(_position);
            CreateCellObject(_position);
        }
    }

    public void DestroyCell(int3 _position)
    {
        if (Grid.IsAlive(_position))
        {
            Grid.RemoveCell(_position);
            DestroyCellObject(_position);
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
        cellPool.ReturnAllObjects();
        cellObjects.Clear();

        Grid = new Grid(gridSize);
        InitializeGrid();

        StatManager.Instance.ResetStats();
        StatManager.Instance.SetPaused(true);
        Debug.Log("Grid and Stats Reset");
    }

    public void IncreaseSpeed()
    {
        float oldInterval = UpdateInterval;
        UpdateInterval -= 0.1f;
        Debug.Log($"Speed Increased - Old: {oldInterval}, New: {UpdateInterval}");
    }

    public void DecreaseSpeed()
    {
        float oldInterval = UpdateInterval;
        UpdateInterval += 0.1f;
        Debug.Log($"Speed Decreased - Old: {oldInterval}, New: {UpdateInterval}");
    }
    #endregion

    #region Private Methods
    private void InitializeGrid()
    {
        Grid = new Grid(gridSize);
        cellObjects = new Dictionary<int3, GameObject>();

        // Example: Create a simple pattern
        //Grid.SetAlive(new int3(4, 4, 4));
        //Grid.SetAlive(new int3(4, 4, 5));
        //Grid.SetAlive(new int3(4, 5, 4));
        //Grid.SetAlive(new int3(5, 4, 4));
        //Grid.SetAlive(new int3(5, 5, 5));
        //Grid.SetAlive(new int3(3, 4, 4));
        //Grid.SetAlive(new int3(4, 3, 4));

        // Render initial cells
        foreach (var cell in Grid.GetActiveCells())
        {
            if (cell.State == CellState.Alive)
            {
                CreateCellObject(cell.Position);
            }
        }

        Debug.Log($"Grid initialized with size: {gridSize}");
    }

    private void InitializeStats()
    {
        if (StatManager.Instance != null)
        {
            StatManager.Instance.InitializeStats();
        }
        else
        {
            Debug.LogError("StatManager instance is null when trying to initialize stats.");
        }
    }

    private void UpdateGrid()
    {
        var cellsToUpdate = Grid.GetActiveCells().ToList();

        foreach (var cell in cellsToUpdate)
        {
            if (cell.State == CellState.Alive || cell.State == CellState.ActiveZone)
            {
                int3 position = cell.Position;
                int aliveNeighbors = Grid.CountAliveNeighbors(position);
                byte newState = DetermineNewState(cell.State, aliveNeighbors);

                if (newState != cell.State)
                {
                    if (newState == CellState.Alive)
                    {
                        Grid.SetAlive(position);

                        if (!cellObjects.ContainsKey(position))
                        {
                            CreateCellObject(position);
                        }
                    }
                    else
                    {
                        Grid.RemoveCell(position);
                        if (cellObjects.ContainsKey(position))
                        {
                            DestroyCellObject(position);
                        }
                    }
                }
            }
        }

        OnCycleComplete?.Invoke();
    }

    private void CalculateAndSetPoolSize(int _gridSize)
    {
        cellPool.Initialize(cellPrefab, cellContainer, _gridSize);
    }

    private byte DetermineNewState(byte _currentState, int _aliveNeighbors)
    {
        if (_currentState == CellState.Alive)
        {
            // A living cell survives if it has 4, 5 or 6 living neighbors
            return (byte)((_aliveNeighbors >= 4 && _aliveNeighbors <= 6) ? CellState.Alive : CellState.Dead);
        }
        else
        {
            // A dead cell is born if it has exactly 4 living neighbors.
            return (byte)(_aliveNeighbors == 4 ? CellState.Alive : CellState.Dead);
        }
    }

    private void CreateCellObject(int3 _position)
    {
        if (!cellObjects.ContainsKey(_position) && cellPool.IsReady)
        {
            Vector3 worldPosition = new Vector3(
                _position.x - (gridSize - 1) / 2f,
                _position.y - (gridSize - 1) / 2f,
                _position.z - (gridSize - 1) / 2f
            ) * CELL_SIZE;

            GameObject cellObject = cellPool.GetObject(worldPosition);
            if (cellObject != null)
            {
                cellObjects[_position] = cellObject;
            }
        }
    }

    private void DestroyCellObject(int3 _position)
    {
        if (cellObjects.TryGetValue(_position, out GameObject cellObject) && cellPool.IsReady)
        {
            cellPool.ReturnObject(cellObject);
            cellObjects.Remove(_position);
        }
    }
    #endregion
}
