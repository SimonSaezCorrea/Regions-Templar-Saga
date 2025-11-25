using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem; // 1. Namespace OBLIGATORIO

public class UnitMovement : MonoBehaviour
{
    Camera cam; // Cámara principal del juego
    NavMeshAgent agent; // Componente NavMeshAgent para mover la unidad
    public LayerMask ground; // Capa del suelo para detectar clics de movimiento
    public bool isCommandedToMove; // Indica si la unidad ha sido comandada a moverse manualmente

    void Start()
    {
        cam = Camera.main;
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
            Debug.LogWarning($"{name}: NavMeshAgent no encontrado.");
    }

    private void Update()
    {
        //// Detectar clic derecho para mover la unidad
        //if (Mouse.current.rightButton.  wasPressedThisFrame)
        //{
        //    RaycastHit hit;
        //    Vector2 mousePos = Mouse.current.position.ReadValue();
        //    Ray ray = cam.ScreenPointToRay(mousePos);

        //    if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
        //    {
        //        isCommandedToMove= true;
        //        agent.SetDestination(hit.point);
        //    }
        //}

        // Verificar si la unidad ha llegado a su destino
        if (agent.hasPath == false|| agent.remainingDistance <= agent.stoppingDistance)
        {
            isCommandedToMove= false;
        }
    }

    public void MoveTo(Vector3 destination)
    {
        isCommandedToMove = true;
        agent.SetDestination(destination);
    }
}