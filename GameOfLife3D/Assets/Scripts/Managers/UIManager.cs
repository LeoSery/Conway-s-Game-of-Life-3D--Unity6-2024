using UnityEngine.UI;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("GameObjects :")]
    public Button PauseButton;
    public Button PlayButton;
    public Button ResetButton;
    public Button SpeedDownButton;
    public Button SpeedUpButton;

    private void Start()
    {
        SetupButtonListeners();
    }

    private void SetupButtonListeners()
    {
        GameManager GM = GameManager.Instance;

        PauseButton.onClick.AddListener(GM.TogglePause);
        PlayButton.onClick.AddListener(GM.TogglePause);
        ResetButton.onClick.AddListener(GM.ResetGrid);
        SpeedDownButton.onClick.AddListener(GM.DecreaseSpeed);
        SpeedUpButton.onClick.AddListener(GM.IncreaseSpeed);
    }
}
