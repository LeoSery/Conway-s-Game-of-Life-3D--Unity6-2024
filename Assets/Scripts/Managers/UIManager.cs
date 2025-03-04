using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;

using TMPro;

public class UIManager : MonoBehaviour
{
    #region Singleton
    public static UIManager Instance { get; private set; }
    #endregion

    #region Public Fields
    [Header("Buttons :")]
    public Button PauseButton;
    public Button PlayButton;
    public Button ResetButton;

    [Header("Config Panel :")]
    public GameObject configPanel;
    public Slider gridSizeSlider;
    public Slider cycleSpeedSlider;
    public TextMeshProUGUI gridSizeText;
    public TextMeshProUGUI cycleSpeedText;

    [Header("Layer Panel :")]
    public GameObject layerPanel;
    public Button LayerUpButton;
    public Button LayerDownButton;
    public Toggle LayerVisibilityToggle;

    [Header("Stats Panel :")]
    public TextMeshProUGUI cycleText;
    public TextMeshProUGUI aliveCellsText;
    public TextMeshProUGUI deadCellsText;
    public TextMeshProUGUI simulationTimeText;
    public TextMeshProUGUI fpsText;
    public TextMeshProUGUI simulationStateText;
    #endregion

    #region Private Fields
    private StatManager statManager;
    private GameManager gameManager;
    private CameraController cameraController;

    private float fpsUpdateInterval = 0.3f;
    private float timeSinceLastFpsUpdate = 0f;
    #endregion

    #region Unity Lifecycle Methods
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
        StatManager.OnStatsUpdate += OnStatsUpdateChanged;
        GameManager.OnPauseStateChanged += OnPauseStateChanged;
    }

    private void OnDisable()
    {
        StatManager.OnStatsUpdate -= OnStatsUpdateChanged;
        GameManager.OnPauseStateChanged -= OnPauseStateChanged;
    }

    private void Start()
    {
        statManager = StatManager.Instance;
        gameManager = GameManager.Instance;
        
        cameraController = gameManager.CameraController;

        SetupButtonListeners();
        SetupConfigPanel();
        SetupGridVisibilityToggle();

        UpdateEditPanelsVisibility(gameManager.IsPaused);
        UpdateStateButtonVisibility(gameManager.IsPaused);
    }
    private void Update()
    {
        UpdateDynamicStatsDisplay();
    }
    #endregion

    #region Public Methods
    public void UpdateDynamicStatsDisplay()
    {
        if (statManager != null)
        {
            simulationTimeText.text = $"Simulation time : {statManager.SimulationTime:F2}s";

            timeSinceLastFpsUpdate += Time.deltaTime;
            if (timeSinceLastFpsUpdate >= fpsUpdateInterval)
            {
                fpsText.text = $"FPS : {statManager.CurrentFPS:F2}";
                timeSinceLastFpsUpdate = 0f;
            }
        }
    }

    public void UpdateCycleStatsDisplay()
    {
        if (statManager != null)
        {
            cycleText.text = $"Cycle : {statManager.CurrentCycle}";
            aliveCellsText.text = $"Alive Cells : {statManager.AliveCells}";
            deadCellsText.text = $"Dead Cells : {statManager.DeadCells}";
        }

        if (gameManager != null)
        {
            UpdateStateButtonVisibility(gameManager.IsPaused);
        }
    }
    #endregion

    #region Private Methods
    private void SetupConfigPanel()
    {
        gridSizeSlider.minValue = gameManager.minGridSize;
        gridSizeSlider.maxValue = gameManager.maxGridSize;
        gridSizeSlider.wholeNumbers = true;
        gridSizeSlider.value = gameManager.gridSize;
        gridSizeSlider.onValueChanged.AddListener(OnGridSizeChanged);

        cycleSpeedSlider.minValue = gameManager.minCycleSpeed;
        cycleSpeedSlider.maxValue = gameManager.maxCycleSpeed;
        cycleSpeedSlider.value = gameManager.UpdateInterval;
        cycleSpeedSlider.onValueChanged.AddListener(OnCycleSpeedChanged);

        UpdateGridSizeText(gameManager.gridSize);
        UpdateCycleSpeedText(gameManager.UpdateInterval);
    }

    private void AssignButtonToAction(Button _button, UnityAction _action)
    {
        if (_button != null && _action != null)
        {
            _button.onClick.AddListener(_action);
        }
        else
        {
            Debug.LogError($"Button : '{(_button != null ? _button.name : null)}' or Action is null");
        }
    }

    private void SetupButtonListeners()
    {
        AssignButtonToAction(PauseButton, gameManager.TogglePause);
        AssignButtonToAction(PlayButton, gameManager.TogglePause);
        AssignButtonToAction(ResetButton, gameManager.ResetGrid);
        AssignButtonToAction(LayerUpButton, gameManager.cellInteractionController.ShowLayer);
        AssignButtonToAction(LayerDownButton, gameManager.cellInteractionController.HideLayer);
    }

    private void SetupGridVisibilityToggle()
    {
        if (LayerVisibilityToggle != null)
        {
            LayerVisibilityToggle.isOn = gameManager.visualGrid.HideGridOnSimulate;
            LayerVisibilityToggle.onValueChanged.AddListener(OnHideGridToggleChanged);
        }
    }

    private void OnHideGridToggleChanged(bool value)
    {
        if (gameManager.visualGrid != null)
        {
            gameManager.visualGrid.SetHideGridWhenPlaying(value);
        }
    }

    private void OnGridSizeChanged(float _value)
    {
        int newSize = Mathf.RoundToInt(_value);
        UpdateGridSizeText(newSize);
        gameManager.ResizeGrid(newSize);
        cameraController.UpdateGridInfo();
    }

    private void OnCycleSpeedChanged(float _value)
    {
        UpdateCycleSpeedText(_value);
        gameManager.UpdateInterval = 1 / _value;
    }

    private void OnStatsUpdateChanged()
    {
        UpdateCycleStatsDisplay();
    }

    private void OnPauseStateChanged(bool _isPaused)
    {
        UpdateStateButtonVisibility(_isPaused);
        UpdateEditPanelsVisibility(_isPaused);
        UpdateStateText(_isPaused);
    }

    private void UpdateGridSizeText(int _size)
    {
        gridSizeText.text = $"{_size}x{_size}x{_size}";
    }

    private void UpdateCycleSpeedText(float _speed)
    {
        cycleSpeedText.text = $"{_speed:F2}";
    }

    private void UpdateEditPanelsVisibility(bool _isPaused)
    {
        if (configPanel != null)
        {
            //configPanel.SetActive(_isPaused);
            //layerPanel.SetActive(_isPaused);
        }
    }

    private void UpdateStateButtonVisibility(bool _isPaused)
    {
        if (PauseButton != null && PlayButton != null)
        {
            PauseButton.gameObject.SetActive(!_isPaused);
            PlayButton.gameObject.SetActive(_isPaused);
        }
    }

    private void UpdateStateText(bool _isPaused)
    {
        simulationStateText.text = _isPaused ? "Simulation Paused" : "Simulation Running";
        simulationStateText.color = _isPaused ? new Color32(222, 95, 95, 255) : new Color32(95, 222, 95, 255);
    }
    #endregion
}
