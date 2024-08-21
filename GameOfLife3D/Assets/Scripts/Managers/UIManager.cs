using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;

using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Bttons :")]
    public Button PauseButton;
    public Button PlayButton;
    public Button ResetButton;
    public Button SpeedDownButton;
    public Button SpeedUpButton;

    //[Header("Config Panel :")]
    //public Slider gridSizeSlider;
    //public Slider cycleSpeedSlider;
    //public TextMeshProUGUI gridSizeText;
    //public TextMeshProUGUI cycleSpeedText;

    [Header("Stats Panel :")]
    public TextMeshProUGUI cycleText;
    public TextMeshProUGUI aliveCellsText;
    public TextMeshProUGUI deadCellsText;
    public TextMeshProUGUI simulationTimeText;
    public TextMeshProUGUI fpsText;

    private StatManager statManager;
    private GameManager gameManager;

    private void OnEnable()
    {
        StatManager.OnStatsUpdate += UpdateCycleStatsDisplay;
    }

    private void OnDisable()
    {
        StatManager.OnStatsUpdate -= UpdateCycleStatsDisplay;
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

    private void Start()
    {
        statManager = StatManager.Instance;
        gameManager = GameManager.Instance;

        SetupButtonListeners();

        //gridSizeSlider.onValueChanged.AddListener(OnGridSizeChanged);
        //cycleSpeedSlider.onValueChanged.AddListener(OnCycleSpeedChanged);
    }
    private void Update()
    {
        UpdateDynamicStatsDisplay();
    }

    //private void OnGridSizeChanged(float value)
    //{
    //    int newSize = Mathf.RoundToInt(value);
    //    gridSizeText.text = $"Grid Size: {newSize}x{newSize}x{newSize}";
    //    GameManager.Instance.ResizeGrid(newSize);
    //}

    //private void OnCycleSpeedChanged(float value)
    //{
    //    cycleSpeedText.text = $"Cycle Speed: {value:F2}";
    //    GameManager.Instance.UpdateInterval = value;
    //}

    private void SetupButtonListeners()
    {
        AssingButtonToAction(PauseButton, gameManager.TogglePause);
        AssingButtonToAction(PlayButton, gameManager.TogglePause);
        AssingButtonToAction(ResetButton, gameManager.ResetGrid);
        AssingButtonToAction(SpeedDownButton, gameManager.DecreaseSpeed);
        AssingButtonToAction(SpeedUpButton, gameManager.IncreaseSpeed);
    }

    private void AssingButtonToAction(Button _button, UnityAction _action)
    {
        if (_button != null && _action != null)
        {
            _button.onClick.AddListener(_action);
        }
        else
        {
            Debug.LogError($"Button : '{_button.name}' or Action : '{_action} is null");
        }
    }

    public void UpdateDynamicStatsDisplay()
    {
        if (statManager != null)
        {
            simulationTimeText.text = $"Simulation time : {statManager.SimulationTime:F2}s";
            fpsText.text = $"FPS : {statManager.CurrentFPS:F2}";
        }
    }

    public void UpdateCycleStatsDisplay()
    {
        if (statManager != null)
        {
            cycleText.text = $"Cycle : {statManager.CurrentCycle}";
            aliveCellsText.text = $"Alive Cells : {statManager.AliveCells}";
            deadCellsText.text = $"Dead Cells : {statManager.DeadCells}";
            Debug.Log($"UpdateCycleStatsDisplay: Alive = {statManager.AliveCells}, Dead = {statManager.DeadCells}");
        }
    }
}
