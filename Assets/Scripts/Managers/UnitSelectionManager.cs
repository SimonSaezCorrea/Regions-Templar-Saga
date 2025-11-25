using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager Instance { get; private set; } // Singleton instance

    public List<GameObject> allUnitsList = new List<GameObject>(); // Lista de todas las unidades en el juego
    public List<GameObject> unitsSelected = new List<GameObject>(); // Lista de unidades actualmente seleccionadas

    public LayerMask clickable; // LayerMask para objetos seleccionables
    public LayerMask ground; // LayerMask para el suelo 
    public LayerMask attackable; // LayerMask para objetos atacables

    public bool attackCursorVisible; // Indica si el cursor de ataque está visible

    public GameObject groundMarker; // Marcador visual para el punto de destino en el suelo

    private Camera cam; // Cámara principal del juego

    // Maneja la selección o deselección de una unidad
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

        // Click izquierdo para seleccionar unidades (usa la máscara clickable)
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            RaycastHit hit; // Almacena información sobre el objeto golpeado
            Vector2 mousePos = Mouse.current.position.ReadValue(); // Posición actual del ratón
            Ray ray = cam.ScreenPointToRay(mousePos); // Crea un rayo desde la cámara a través de la posición del ratón

            // Realiza el raycast y verifica si golpea un objeto en la máscara clickable
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickable))
            {
                // Si se mantiene Shift, multi-selección; de lo contrario, selección única
                if (shiftHeld) MultiSelect(hit.collider.gameObject);
                // Si no se mantiene Shift, selección única
                else SelectByClicking(hit.collider.gameObject);
            }
            // Si no se golpea ningún objeto, deseleccionar todo (a menos que se mantenga Shift)
            else
            {
                if (!shiftHeld) DeselectAll();
            }
        }

        // Click derecho para mover unidades (usa la máscara ground)
        if (Mouse.current.rightButton.wasPressedThisFrame && unitsSelected.Count > 0)
        {
            RaycastHit hit; // Almacena información sobre el objeto golpeado
            Vector2 mousePos = Mouse.current.position.ReadValue(); // Posición actual del ratón
            Ray ray = cam.ScreenPointToRay(mousePos); // Crea un rayo desde la cámara a través de la posición del ratón

            // Realiza el raycast y verifica si golpea un objeto en la máscara ground
            if (AtleastOneOffensiveUnit(unitsSelected) && Physics.Raycast(ray, out hit, Mathf.Infinity, attackable))
            {
                Debug.Log("Attackable hit: " + hit.collider.gameObject.name);
                Transform target = hit.transform;

                // 1. SI ES ATAQUE: Asignar objetivo de ataque a todas las unidades seleccionadas
                foreach (GameObject unit in unitsSelected)
                {
                    if (unit.TryGetComponent<AttackController>(out var attackController))
                    {
                        attackController.targetToAttack = target;
                    }

                    // IMPORTANTE: Cancelamos el "movimiento forzado" para que el StateMachine tome el control
                    if (unit.TryGetComponent<UnitMovement>(out var movement))
                    {
                        movement.isCommandedToMove = false;
                    }
                }
            }
            // 2. SI NO ES ATAQUE: Intentar mover al suelo
            else if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
            {
                // Mostrar el marcador de destino en el suelo
                if (groundMarker != null)
                {
                    groundMarker.transform.position = hit.point;
                    groundMarker.SetActive(true);
                }

                // Ordenar a todas las unidades seleccionadas que se muevan al punto golpeado
                foreach (GameObject unit in unitsSelected)
                {
                    // Limpiamos el objetivo de ataque al dar una orden de movimiento
                    if (unit.TryGetComponent<AttackController>(out var attackController))
                    {
                        attackController.targetToAttack = null;
                    }

                    // Usamos el nuevo método público
                    if (unit.TryGetComponent<UnitMovement>(out var movement))
                    {
                        movement.MoveTo(hit.point);
                    }
                }
            }
        }

        // Lógica visual del cursor de ataque (separada del input de acción para mantenerlo limpio)
        HandleAttackCursorVisibility();
    }

    // Maneja la visibilidad del cursor de ataque
    private void HandleAttackCursorVisibility()
    {
        if (unitsSelected.Count > 0 && AtleastOneOffensiveUnit(unitsSelected))
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            attackCursorVisible = Physics.Raycast(ray, out _, Mathf.Infinity, attackable);
        }
        else
        {
            attackCursorVisible = false;
        }
    }


    // Verifica si al menos una unidad seleccionada tiene un componente AttackController
    // Es decir si es una unidad ofensiva
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

    // Maneja la multi-selección de unidades
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

    // Deselecciona todas las unidades actualmente seleccionadas
    private void DeselectAll()
    {
        // iterate a copy to be safe if something mutates list during callbacks
        for (int i = unitsSelected.Count - 1; i >= 0; i--)
        {
            SelectedUnit(unitsSelected[i], false);
        }
        unitsSelected.Clear();

        if (groundMarker != null) groundMarker.SetActive(false);
    }

    // Selecciona una unidad al hacer clic en ella
    private void SelectByClicking(GameObject unit)
    {
        if (unit == null) return;

        DeselectAll();

        SelectedUnit(unit, true);
    }

    // Habilita o deshabilita el movimiento de una unidad
    private void EnableUnitMovement(GameObject unit, bool shouldMove)
    {
        if (unit == null) return;

        if (unit.TryGetComponent<UnitMovement>(out var um))
        {
            um.enabled = shouldMove;
        }
    }

    // Activa o desactiva el indicador de selección de una unidad
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

    // Método para selección mediante arrastre (drag select)
    internal void DragSelect(GameObject unit)
    {
        if (!unitsSelected.Contains(unit))
        {
            SelectedUnit(unit, true);
        }
    }
}