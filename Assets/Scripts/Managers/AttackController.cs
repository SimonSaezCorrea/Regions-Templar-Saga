using UnityEngine;

public class AttackController : MonoBehaviour
{
    public Transform targetToAttack;

    [Header("Settings")]
    public Material idleStateMaterial; // Material para el estado de inactividad
    public Material followStateMaterial; // Material para el estado de seguimiento
    public Material attackStateMaterial; // Material para el estado de ataque

    [Header("Debug Mode")]
    public bool active;
    public bool activeGizmosFollowArea;
    public bool activeGizmosAttackArea;
    public bool activeGizmosStopAttackingArea;

    private Renderer myRenderer; // Caché del renderer para no buscarlo cada vez

    private void Start()
    {
        myRenderer = GetComponent<Renderer>();
    }


    // Detecta cuando un enemigo entra en el área de ataque
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && targetToAttack == null)
        {
            targetToAttack = other.transform;
        }
    }

    // Detecta cuando un enemigo sale del área de ataque
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy") && targetToAttack != null)
        {
            if (other.transform == targetToAttack)
            {
                targetToAttack = null;
            }
        }
    }


    // ----------- Visuals (Optimizados con caché) ------------

    public void SetIdleMaterial()
    {
        if(active == false) return;

        if(myRenderer) myRenderer.material = idleStateMaterial;
    }

    public void SetFollowMaterial()
    {
        if (active == false) return;

        if (myRenderer) myRenderer.material = followStateMaterial;
    }

    public void SetAttackMaterial()
    {
        if (active == false) return;

        if (myRenderer) myRenderer.material = attackStateMaterial;
    }

    public void OnDrawGizmos()
    {
        if (active == false) return;

        if (activeGizmosFollowArea == true)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 10f * 0.3f);
        }

        if (activeGizmosStopAttackingArea == true)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 1.2f);
        }
        if (activeGizmosAttackArea == true)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}
