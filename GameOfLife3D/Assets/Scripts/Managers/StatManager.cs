using System.Linq;
using UnityEngine;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance { get; private set; }

    public delegate void StatsUpdateHandler();
    public static event StatsUpdateHandler OnStatsUpdate;

    [SerializeField]
    private int currentCycle = 0;
    public int CurrentCycle => currentCycle;

    [SerializeField]
    private int aliveCells;
    public int AliveCells => aliveCells;

    [SerializeField]
    private int totalCells;
    public int TotalCells => totalCells;

    [SerializeField]
    private float simulationTime;
    public float SimulationTime => simulationTime;

    [SerializeField]
    private float currentFPS;
    public float CurrentFPS => currentFPS;

    public int DeadCells => totalCells - aliveCells;

    private float fpsUpdateInterval = 0.5f;
    private float fpsTimer = 0f;
    private bool isPaused = true;

    private void OnEnable()
    {
        GameManager.OnCycleComplete += UpdateCycleStats;
    }

    private void OnDisable()
    {
        GameManager.OnCycleComplete -= UpdateCycleStats;

    }

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

    private void Update()
    {
        if (!isPaused)
        {
            simulationTime += Time.deltaTime;
            UpdateFPS();
        }
    }

    public void InitializeStats()
    {
        currentCycle = 0;
        aliveCells = GetAliveCells();
        totalCells = GetTotalCells();
        simulationTime = 0f;
        currentFPS = 0f;

        OnStatsUpdate?.Invoke();
    }

    private void UpdateCycleStats()
    {
        currentCycle++;
        aliveCells = GetAliveCells();

        OnStatsUpdate?.Invoke();
    }

    private void UpdateFPS()
    {
        fpsTimer += Time.unscaledDeltaTime;

        if (fpsTimer >= fpsUpdateInterval)
        {
            currentFPS = 1f / Time.unscaledDeltaTime;
            fpsTimer = 0f;
        }
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }

    public void ResetStats()
    {
        currentCycle = 0;
        aliveCells = 0;
        totalCells = 0;
        simulationTime = 0f;
        currentFPS = 0f;
        fpsTimer = 0f;

        OnStatsUpdate?.Invoke();
    }

    private int GetAliveCells()
    {
        if (GameManager.Instance.Grid != null)
        {
            int count = GameManager.Instance.Grid.GetActiveCells().Count(c => c.State == CellState.Alive);
            return count;
        }
        else
        {
            Debug.LogWarning("GetAliveCells: Grid is null");
            return 0;
        }
    }

    private int GetTotalCells()
    {
        if (GameManager.Instance.Grid != null)
        {
            return (int)Mathf.Pow(GameManager.Instance.gridSize, 3);
        }
        else
        {
            Debug.LogWarning("GetTotalCells: Grid is null");
            return 0;
        }
    }
}
