using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem; // 1. Namespace OBLIGATORIO

public class UnitMovement : MonoBehaviour
{
    Camera cam;
    NavMeshAgent agent;
    public LayerMask ground;
    public bool isCommandedToMove;

    void Start()
    {
        cam = Camera.main;
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
            Debug.LogWarning($"{name}: NavMeshAgent no encontrado.");
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            RaycastHit hit;
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
            {
                isCommandedToMove= true;
                agent.SetDestination(hit.point);
            }
        }

        if(!agent.hasPath && agent.remainingDistance <= agent.stoppingDistance)
        {
            isCommandedToMove= false;
        }
    }
}