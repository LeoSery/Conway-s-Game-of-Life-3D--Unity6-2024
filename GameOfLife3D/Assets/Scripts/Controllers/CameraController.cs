using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region Public Fields
    public Camera mainCamera;

    [Header("Mouse Sensitivity")]
    public float mouseHorizontalSensitivity = 2f;
    public float mouseVerticalSensitivity = 2f;

    [Header("Movement Speed")]
    public float horizontalMoveSpeed = 5f;
    public float verticalMoveSpeed = 5f;

    [Header("Movement Limits")]
    public float additionalDistanceFromGrid = 5f;
    #endregion

    #region Private Fields
    private float horizontalRotation = 0f;
    private float verticalRotation = 0f;
    private bool isActive = true;
    private float maxDistanceFromCenter;
    #endregion

    #region Unity Lifecycle Methods
    private void Start()
    {
        if (InputManager.Instance == null)
        {
            Debug.LogError("InputManager instance is null. Make sure it's initialized before CameraController.");
            return;
        }

        InputManager.Instance.OnMouseLook += HandleMouseLook;
        InputManager.Instance.OnMove += HandleMovement;
        InputManager.Instance.OnFocusChanged += HandleFocusChanged;

        if (mainCamera == null)
        {
            Debug.LogError("Main camera reference is not set. Please assign the main camera in the inspector.");
        }
        else
        {
            Vector3 rotation = mainCamera.transform.eulerAngles;
            horizontalRotation = rotation.y;
            verticalRotation = rotation.x;
        }

        UpdateGridInfo();
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMouseLook -= HandleMouseLook;
            InputManager.Instance.OnMove -= HandleMovement;
            InputManager.Instance.OnFocusChanged -= HandleFocusChanged;
        }
    }
    #endregion

    #region Pubic Methods
    public void OnGridSizeChanged()
    {
        UpdateGridInfo();
    }

    public void UpdateGridInfo()
    {
        maxDistanceFromCenter = (GameManager.Instance.gridSize / 2f) + additionalDistanceFromGrid;
    }
    #endregion

    #region Private Methods
    private void HandleFocusChanged(bool _isFocused)
    {
        isActive = _isFocused;
    }

    private void HandleMouseLook(Vector2 _mouseDelta)
    {
        if (!isActive || mainCamera == null) return;

        float deltaX = _mouseDelta.x * mouseHorizontalSensitivity;
        float deltaY = _mouseDelta.y * mouseVerticalSensitivity;

        horizontalRotation += deltaX;
        verticalRotation -= deltaY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        mainCamera.transform.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
    }

    private void HandleMovement(Vector3 _movement)
    {
        if (!isActive || mainCamera == null) return;

        Vector3 horizontalMove = horizontalMoveSpeed * _movement.x * mainCamera.transform.right;
        Vector3 verticalMove = _movement.z * verticalMoveSpeed * mainCamera.transform.forward;
        Vector3 targetPosition = (horizontalMove + verticalMove) * Time.deltaTime;

        Vector3 finalPositon = mainCamera.transform.position + targetPosition;

        if (IsWithinBounds(finalPositon))
        {
            mainCamera.transform.position = finalPositon;
        }
        else
        {
            mainCamera.transform.position = GetClosestValidPosition(finalPositon);
        }
    }

    private bool IsWithinBounds(Vector3 _position)
    {
        return _position.magnitude <= maxDistanceFromCenter;
    }

    private Vector3 GetClosestValidPosition(Vector3 _position)
    {
        return _position.normalized * maxDistanceFromCenter;
    }
    #endregion
}
