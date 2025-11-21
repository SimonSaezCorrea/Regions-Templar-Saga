using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }

    public List<GameObject> allUnitsList = new List<GameObject>();
    public List<GameObject> unitsSelected = new List<GameObject>();

    public LayerMask clickable;
    public LayerMask ground;
    public LayerMask attackable;
    public bool attackCursorVisible;
    public GameObject groundMarker;

    private Camera cam;

    private void SelectedUnit(GameObject unit, bool isSelected)
    {
        if (unit == null) return;

        if (!isSelected)
            unitsSelected.Remove(unit);
        else 
        {
            unitsSelected.Add(unit);
        }
        TriggerSelectionIndicator(unit, isSelected);
        EnableUnitMovement(unit, isSelected);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        bool shiftHeld = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

        // Left click selection
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            RaycastHit hit;
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickable))
            {
                if (shiftHeld)
                {
                    MultiSelect(hit.collider.gameObject);
                }
                else
                {
                    SelectByClicking(hit.collider.gameObject);
                }
            }
            else
            {
                if (!shiftHeld)
                {
                    DeselectAll();
                }
            }
        }

        // Right click: show ground marker (uses ground mask)
        if (Mouse.current.rightButton.wasPressedThisFrame && unitsSelected.Count > 0)
        {
            RaycastHit hit;
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
            {
                if (groundMarker != null)
                {
                    groundMarker.transform.position = hit.point;
                    groundMarker.SetActive(true);
                }
            }
        }

        // Attack command: hide ground marker
        if (unitsSelected.Count > 0 && AtleastOneOffensiveUnit(unitsSelected))
        {
            RaycastHit hit;
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, attackable))
            {
                Debug.Log("Attackable hit: " + hit.collider.gameObject.name);

                attackCursorVisible = true;

                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    Transform target = hit.transform;

                    foreach (GameObject unit in unitsSelected)
                    {
                        if (unit.GetComponent<AttackController>())
                        {
                            unit.GetComponent<AttackController>().tarjetToAttack = target;
                        }
                    }
                }
            }
        }
        else
        {
            attackCursorVisible = false;
        }
    }

    private bool AtleastOneOffensiveUnit(List<GameObject> selectedUnits)
    {
        foreach (GameObject unit in selectedUnits)
        {
            if (unit.GetComponent<AttackController>())
            {
                return true;
            }
        }
        return false;
    }

    private void MultiSelect(GameObject unit)
    {
        if (unit == null) return;

        if (unitsSelected.Contains(unit))
        {
            SelectedUnit(unit, false);
        }
        else
        {
            SelectedUnit(unit, true);
        }
    }

    private void DeselectAll()
    {
        // iterate a copy to be safe if something mutates list during callbacks
        foreach (var unit in new List<GameObject>(unitsSelected))
        {
            SelectedUnit(unit, false);
        }

        if (groundMarker != null)
            groundMarker.SetActive(false);

        unitsSelected.Clear();
    }

    private void SelectByClicking(GameObject unit)
    {
        if (unit == null) return;

        DeselectAll();

        SelectedUnit(unit, true);
    }

    private void EnableUnitMovement(GameObject unit, bool shouldMove)
    {
        if (unit == null) return;

        if (unit.TryGetComponent<UnitMovement>(out var um))
        {
            um.enabled = shouldMove;
        }
    }

    private void TriggerSelectionIndicator(GameObject unit, bool isVisible)
    {
        if (unit == null) return;

        // safe child access: assume indicator is first child but avoid exceptions
        if (unit.transform.childCount > 0)
        {
            var indicator = unit.transform.GetChild(0).gameObject;
            if (indicator != null)
                indicator.SetActive(isVisible);
        }
    }

    internal void DragSelect(GameObject unit)
    {
        if (!unitsSelected.Contains(unit))
        {
            SelectedUnit(unit, true);
        }
    }
}