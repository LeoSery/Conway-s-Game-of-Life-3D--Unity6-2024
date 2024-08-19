using System.Collections.Generic;

using UnityEngine;

using Unity.Mathematics;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Settings :")]
    public float minUpdateInterval = 0.1f;
    public float maxUpdateInterval = 2f;

    [SerializeField, Range(0.1f, 2f)]
    private float updateInterval = 1f;

    public float UpdateInterval
    {
        get => updateInterval;
        set
        {
            updateInterval = Mathf.Clamp(value, minUpdateInterval, maxUpdateInterval);
        }
    }

    [Header("Prefabs :")]
    public GameObject cellPrefab;
    public Transform cellContainer;

    private Grid grid;
    private Dictionary<int3, GameObject> cellObjects;

    private float lastUpdateTime;
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
        grid = new Grid();
        cellObjects = new Dictionary<int3, GameObject>();
        InitializeGrid();
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
        GameObject cellObject = Instantiate(cellPrefab, new Vector3(position.x, position.y, position.z), Quaternion.identity, cellContainer);
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
        grid = new Grid();
        InitializeGrid();

        Debug.Log("Grid Reset");
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
