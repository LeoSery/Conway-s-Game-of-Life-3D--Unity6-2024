using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera mainCamera;
    [Header("Mouse Sensitivity")]
    public float mouseHorizontalSensitivity = 2f;
    public float mouseVerticalSensitivity = 2f;
    [Header("Movement Speed")]
    public float horizontalMoveSpeed = 5f;
    public float verticalMoveSpeed = 5f;

    private float horizontalRotation = 0f;
    private float verticalRotation = 0f;
    private bool isActive = true;

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

        Debug.LogWarning($"Camera position: {mainCamera.transform.position}, rotation: {mainCamera.transform.rotation.eulerAngles}");

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

    private void HandleFocusChanged(bool isFocused)
    {
        isActive = isFocused;
    }

    private void HandleMouseLook(Vector2 mouseDelta)
    {
        if (!isActive || mainCamera == null) return;

        float deltaX = mouseDelta.x * mouseHorizontalSensitivity;
        float deltaY = mouseDelta.y * mouseVerticalSensitivity;

        horizontalRotation += deltaX;
        verticalRotation -= deltaY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        mainCamera.transform.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
    }

    private void HandleMovement(Vector3 movement)
    {
        if (!isActive || mainCamera == null) return;

        Vector3 horizontalMove = horizontalMoveSpeed * movement.x * mainCamera.transform.right;
        Vector3 verticalMove = movement.z * verticalMoveSpeed * mainCamera.transform.forward;
        Vector3 finalMove = (horizontalMove + verticalMove) * Time.deltaTime;

        mainCamera.transform.position += finalMove;
    }
}