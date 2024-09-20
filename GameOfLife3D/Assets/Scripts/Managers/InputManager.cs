using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public event Action<Vector2> OnMouseLook;
    public event Action<Vector3> OnMove;

    public event Action OnPlaceCell;
    public event Action OnRemoveCell;

    public event Action<bool> OnFocusChanged;

    public event Action OnShowLayer;
    public event Action OnHideLayer;

    private bool ignoreNextMouseMovement = false;
    private bool isFocused = false;

    private GraphicRaycaster graphicRaycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;

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

        graphicRaycaster = FindObjectOfType<Canvas>().GetComponent<GraphicRaycaster>();
        eventSystem = FindObjectOfType<EventSystem>();
    }

    private void Start()
    {
        SetFocus(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleFocus();
        }

        if (!isFocused && Input.GetMouseButtonDown(0))
        {
            CheckForUIClick();
        }

        if (isFocused)
        {
            if (!ignoreNextMouseMovement)
            {
                HandleMouseLook();
            }
            else
            {
                ignoreNextMouseMovement = false;
            }
            HandleMovement();
            HandleCellInteraction();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            OnShowLayer?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            OnHideLayer?.Invoke();
        }
    }

    private void CheckForUIClick()
    {
        pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, results);

        if (results.Count == 0)
        {
            SetFocus(true);
        }
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        OnMouseLook?.Invoke(new Vector2(mouseX, mouseY));
    }

    private void HandleMovement()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        OnMove?.Invoke(new Vector3(moveHorizontal, 0, moveVertical));
    }

    private void HandleCellInteraction()
    {
        if (Input.GetMouseButtonDown(1))
        {
            OnPlaceCell?.Invoke();
            Debug.Log("Place Cell event triggered");
        }
        else if (Input.GetMouseButtonDown(0))
        {
            OnRemoveCell?.Invoke();
            Debug.Log("Remove Cell event triggered");
        }
    }

    public void ToggleFocus()
    {
        SetFocus(!isFocused);
    }

    public void SetFocus(bool focused)
    {
        if (isFocused != focused)
        {
            isFocused = focused;

            if (isFocused)
            {
                ignoreNextMouseMovement = true;
            }

#if UNITY_EDITOR
            Cursor.visible = !isFocused;
            //Debug.Log($"Editor mode: SetFocus called. isFocused: {isFocused}, Cursor visible: {Cursor.visible}");
#else
            Cursor.visible = !isFocused;
            Cursor.lockState = isFocused ? CursorLockMode.Locked : CursorLockMode.None;
#endif

            OnFocusChanged?.Invoke(isFocused);
        }
    }
}
