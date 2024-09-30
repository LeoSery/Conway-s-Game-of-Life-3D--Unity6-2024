using System.Linq;

using UnityEngine;

public class StatManager : MonoBehaviour
{
    #region Singleton
    public static StatManager Instance { get; private set; }
    #endregion

    #region Events
    public delegate void StatsUpdateHandler();
    public static event StatsUpdateHandler OnStatsUpdate;
    #endregion

    #region Public Fields And Properties
    [SerializeField] private int currentCycle = 0;
    public int CurrentCycle => currentCycle;

    [SerializeField] private int aliveCells;
    public int AliveCells => aliveCells;

    [SerializeField] private int totalCells;
    public int TotalCells => totalCells;

    [SerializeField] private float simulationTime;
    public float SimulationTime => simulationTime;

    [SerializeField] private float currentFPS;
    public float CurrentFPS => currentFPS;

    public int DeadCells => Mathf.Max(0, totalCells - aliveCells);
    #endregion

    #region Private Fields
    private readonly float fpsUpdateInterval = 0.5f;
    private float fpsTimer = 0f;
    private bool isPaused = true;
    #endregion

    #region Lifecycle Methods
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

    private void OnEnable()
    {
        GameManager.OnCycleComplete += UpdateCycleStats;
    }

    private void OnDisable()
    {
        GameManager.OnCycleComplete -= UpdateCycleStats;

    }

    private void Update()
    {
        if (!isPaused)
        {
            simulationTime += Time.deltaTime;
            UpdateFPS();
        }
    }
    #endregion

    #region Public Methods
    public void InitializeStats()
    {
        currentCycle = 0;
        aliveCells = GetAliveCells();
        totalCells = GetTotalCells();
        simulationTime = 0f;
        currentFPS = 0f;

        OnStatsUpdate?.Invoke();
    }

    public void UpdateTotalCells()
    {
        totalCells = GetTotalCells();
    }

    public void SetPaused(bool _paused)
    {
        isPaused = _paused;
    }

    public void ResetStats()
    {
        currentCycle = 0;
        aliveCells = 0;
        totalCells = GetTotalCells();
        simulationTime = 0f;
        currentFPS = 0f;
        fpsTimer = 0f;

        OnStatsUpdate?.Invoke();
    }
    #endregion

    #region Private Methods
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
    #endregion
}
