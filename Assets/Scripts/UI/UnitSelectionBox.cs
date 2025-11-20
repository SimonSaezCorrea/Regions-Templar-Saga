using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionBox : MonoBehaviour
{
    Camera myCam;

    [SerializeField]
    RectTransform boxVisual;

    Rect selectionBox;

    Vector2 startPosition;
    Vector2 endPosition;

    private void Start()
    {
        myCam = Camera.main;
        startPosition = Vector2.zero;
        endPosition = Vector2.zero;
        if (boxVisual != null)
            boxVisual.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Mouse.current == null) return;
        if (boxVisual == null) return;

        // When Clicked (pressed this frame)
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            startPosition = Mouse.current.position.ReadValue();
            endPosition = startPosition;
            selectionBox = new Rect();

            boxVisual.gameObject.SetActive(true);
            DrawVisual();
        }

        // While dragging
        if (Mouse.current.leftButton.isPressed)
        {
            endPosition = Mouse.current.position.ReadValue();
            DrawVisual();
            DrawSelection();
        }

        // When Releasing
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            SelectUnits();

            startPosition = Vector2.zero;
            endPosition = Vector2.zero;
            DrawVisual();

            boxVisual.gameObject.SetActive(false);
        }
    }

    void DrawVisual()
    {
        // Calculate the starting and ending positions of the selection box.
        Vector2 boxStart = startPosition;
        Vector2 boxEnd = endPosition;

        // Calculate the center of the selection box.
        Vector2 boxCenter = (boxStart + boxEnd) / 2f;

        // Set the position of the visual selection box based on its center.
        boxVisual.position = boxCenter;

        // Calculate the size of the selection box in both width and height.
        Vector2 boxSize = new Vector2(Mathf.Abs(boxStart.x - boxEnd.x), Mathf.Abs(boxStart.y - boxEnd.y));

        // Set the size of the visual selection box based on its calculated size.
        boxVisual.sizeDelta = boxSize;
    }

    void DrawSelection()
    {
        // Use the startPosition and endPosition (both in screen coordinates) to build the rect
        // X
        if (endPosition.x < startPosition.x)
        {
            selectionBox.xMin = endPosition.x;
            selectionBox.xMax = startPosition.x;
        }
        else
        {
            selectionBox.xMin = startPosition.x;
            selectionBox.xMax = endPosition.x;
        }

        // Y
        if (endPosition.y < startPosition.y)
        {
            selectionBox.yMin = endPosition.y;
            selectionBox.yMax = startPosition.y;
        }
        else
        {
            selectionBox.yMin = startPosition.y;
            selectionBox.yMax = endPosition.y;
        }
    }

    void SelectUnits()
    {
        if (UnitSelectionManager.Instance == null) return;
        if (myCam == null) myCam = Camera.main;
        if (myCam == null) return;

        foreach (var unit in UnitSelectionManager.Instance.allUnitsList)
        {
            Vector3 screenPos = myCam.WorldToScreenPoint(unit.transform.position);
            Vector2 screenPos2 = new Vector2(screenPos.x, screenPos.y);

            if (selectionBox.Contains(screenPos2))
            {
                UnitSelectionManager.Instance.DragSelect(unit);
            }
        }
    }
}