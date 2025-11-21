using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    // If we want to select an item to follow, inside the item script add:
    // public void OnMouseDown(){
    //   CameraController.instance.followTransform = transform;
    // }

    [Header("General")]
    [SerializeField] Transform cameraTransform;
    public Transform followTransform;
    Vector3 newPosition;
    Vector3 dragStartPosition;
    Vector3 dragCurrentPosition;

    [Header("Optional Functionality")]
    [SerializeField] bool moveWithKeyboad;
    [SerializeField] bool moveWithEdgeScrolling;
    [SerializeField] bool moveWithMouseDrag;

    [Header("Keyboard Movement")]
    [SerializeField] float normalSpeed = 0.05f;
    [SerializeField] float movementSensitivity = 20f; // Hardcoded Sensitivity
    float movementSpeed;

    [Header("Edge Scrolling Movement")]
    [SerializeField] float edgeSize = 50f;
    bool isCursorSet = false;
    public Texture2D cursorArrowUp;
    public Texture2D cursorArrowDown;
    public Texture2D cursorArrowLeft;
    public Texture2D cursorArrowRight;

    [Header("Zoom")]
    [SerializeField] float zoomSpeed = 5f;
    [SerializeField] float minHeight = 20f;
    [SerializeField] float maxHeight = 70f;

    CursorArrow currentCursor = CursorArrow.DEFAULT;
    enum CursorArrow { UP, DOWN, LEFT, RIGHT, DEFAULT }

    private void Start()
    {
        instance = this;

        // Use cameraTransform if assigned, otherwise fallback to this object's transform
        var target = cameraTransform != null ? cameraTransform : transform;

        newPosition = target.position;

        movementSpeed = normalSpeed;
    }

    private void Update()
    {
        var target = cameraTransform != null ? cameraTransform : transform;

        // Allow Camera to follow Target
        if (followTransform != null)
        {
            target.position = followTransform.position;

            // keep newPosition in sync so manual movement resumes smoothly
            newPosition = target.position;
        }
        // Let us control Camera
        else
        {
            HandleCameraMovement();
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.isPressed)
        {
            followTransform = null;
        }
    }

    void HandleCameraMovement()
    {
        var target = cameraTransform != null ? cameraTransform : transform;

        // Ensure movementSpeed has a sensible default
        if (movementSpeed <= 0f)
            movementSpeed = normalSpeed;

        // Calculate planar directions (ignore camera tilt) so moving up/down doesn't zoom
        Vector3 planarForward = Vector3.ProjectOnPlane(target.forward, Vector3.up).normalized;
        Vector3 planarRight = Vector3.ProjectOnPlane(target.right, Vector3.up).normalized;

        // Mouse Drag
        if (moveWithMouseDrag)
            HandleMouseDragInput();

        // Keyboard Control
        if (moveWithKeyboad && Keyboard.current != null)
        {
            movementSpeed = normalSpeed;

            if (Keyboard.current.upArrowKey.isPressed)
                newPosition += (planarForward * movementSpeed);
            if (Keyboard.current.downArrowKey.isPressed)
                newPosition += (planarForward * -movementSpeed);
            if (Keyboard.current.rightArrowKey.isPressed)
                newPosition += (planarRight * movementSpeed);
            if (Keyboard.current.leftArrowKey.isPressed)
                newPosition += (planarRight * -movementSpeed);
        }

        // Edge Scrolling
        if (moveWithEdgeScrolling && Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            float mouseX = mousePos.x;
            float mouseY = mousePos.y;

            bool movedThisFrame = false;

            // Move Right
            if (mouseX > Screen.width - edgeSize)
            {
                newPosition += (planarRight * movementSpeed);
                ChangeCursor(CursorArrow.RIGHT);
                isCursorSet = true;
                movedThisFrame = true;
            }
            // Move Left
            else if (mouseX < edgeSize)
            {
                newPosition += (planarRight * -movementSpeed);
                ChangeCursor(CursorArrow.LEFT);
                isCursorSet = true;
                movedThisFrame = true;
            }

            // Move Up
            if (mouseY > Screen.height - edgeSize)
            {
                newPosition += (planarForward * movementSpeed);
                ChangeCursor(CursorArrow.UP);
                isCursorSet = true;
                movedThisFrame = true;
            }
            // Move Down
            else if (mouseY < edgeSize)
            {
                newPosition += (planarForward * -movementSpeed);
                ChangeCursor(CursorArrow.DOWN);
                isCursorSet = true;
                movedThisFrame = true;
            }

            if (!movedThisFrame && isCursorSet)
            {
                ChangeCursor(CursorArrow.DEFAULT);
                isCursorSet = false;
            }
        }

        // Zoom with mouse wheel
        if (Mouse.current != null)
        {
            Vector2 scroll = Mouse.current.scroll.ReadValue();
            if (Mathf.Abs(scroll.y) > 0.001f)
            {
                // Calculate proposed position after zoom
                Vector3 proposed = newPosition + (target.forward * scroll.y * zoomSpeed);

                // Only apply zoom if proposed height is within bounds
                if (proposed.y >= minHeight && proposed.y <= maxHeight)
                    newPosition = proposed;

                // If proposed is out of bounds, do nothing (stay still)
            }
        }

        // Smoothly move camera transform   
        target.position = Vector3.Lerp(target.position, newPosition, Time.deltaTime * movementSensitivity);

        Cursor.lockState = CursorLockMode.Confined; // If we have an extra monitor we don't want to exit screen bounds
    }

    private void ChangeCursor(CursorArrow newCursor)
    {
        // Only change cursor if its not the same cursor
        if (currentCursor != newCursor)
        {
            Vector2 hotspot;
            switch (newCursor)
            {
                case CursorArrow.UP:
                    if (cursorArrowUp != null)
                    {
                        hotspot = new Vector2(cursorArrowUp.width * 0.5f, cursorArrowUp.height * 0.5f);
                        Cursor.SetCursor(cursorArrowUp, hotspot, CursorMode.Auto);
                    }
                    break;
                case CursorArrow.DOWN:
                    if (cursorArrowDown != null)
                    {
                        hotspot = new Vector2(cursorArrowDown.width * 0.5f, cursorArrowDown.height * 0.5f);
                        Cursor.SetCursor(cursorArrowDown, hotspot, CursorMode.Auto); // center hotspot
                    }
                    break;
                case CursorArrow.LEFT:
                    if (cursorArrowLeft != null)
                    {
                        hotspot = new Vector2(cursorArrowLeft.width * 0.5f, cursorArrowLeft.height * 0.5f);
                        Cursor.SetCursor(cursorArrowLeft, hotspot, CursorMode.Auto);
                    }
                    break;
                case CursorArrow.RIGHT:
                    if (cursorArrowRight != null)
                    {
                        hotspot = new Vector2(cursorArrowRight.width * 0.5f, cursorArrowRight.height * 0.5f);
                        Cursor.SetCursor(cursorArrowRight, hotspot, CursorMode.Auto); // center hotspot
                    }
                    break;
                case CursorArrow.DEFAULT:
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    break;
            }

            currentCursor = newCursor;
        }
    }

    private void HandleMouseDragInput()
    {
        if (Mouse.current == null)
            return;

        // Don't drag when pointer is over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // Use cameraTransform if available
        var targetCamera = Camera.main;
        if (targetCamera == null)
            return;

        // Start drag on the frame the middle button was pressed
        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Vector2 mp = Mouse.current.position.ReadValue();
            Ray ray = targetCamera.ScreenPointToRay(new Vector3(mp.x, mp.y, 0f));

            if (plane.Raycast(ray, out float entry))
                dragStartPosition = ray.GetPoint(entry);
        }

        // While middle button is held, update drag
        if (Mouse.current.middleButton.isPressed)
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Vector2 mp = Mouse.current.position.ReadValue();
            Ray ray = targetCamera.ScreenPointToRay(new Vector3(mp.x, mp.y, 0f));

            if (plane.Raycast(ray, out float entry))
            {
                dragCurrentPosition = ray.GetPoint(entry);

                var target = cameraTransform != null ? cameraTransform : transform;
                newPosition = target.position + dragStartPosition - dragCurrentPosition;
            }
        }
    }
}