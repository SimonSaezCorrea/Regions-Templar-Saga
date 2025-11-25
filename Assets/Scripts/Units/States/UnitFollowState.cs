using UnityEngine;
using UnityEngine.AI;

public class UnitFollowState : StateMachineBehaviour
{

    AttackController attackController; // Referencia al controlador de ataque de la unidad
    NavMeshAgent agent; // Componente NavMeshAgent para mover la unidad

    public float attackingDistance = 1f; // Distancia a la que la unidad comenzará a atacar

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent = animator.GetComponent<NavMeshAgent>();
        if (animator.TryGetComponent<AttackController>(out attackController))
        {
            attackController.SetFollowMaterial();
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (attackController == null) return;

        // ------  Verificar si hay un objetivo para atacar -------
        // Si no hay un objetivo, transicionar a estado de inactividad
        if (attackController.targetToAttack == null)
        {
            animator.SetBool("isFollowing", false); 
        }
        // Si hay un objetivo, mover la unidad hacia él
        else
        {
            var movement = animator.GetComponent<UnitMovement>();
            // Mover la unidad hacia el objetivo solo si no ha sido comandada a moverse manualmente
            if (movement != null && movement.isCommandedToMove == false)
            {
                agent.SetDestination(attackController.targetToAttack.position); // Mover la unidad hacia el objetivo
                animator.transform.LookAt(attackController.targetToAttack); // Hacer que la unidad mire hacia el objetivo

                // Verificar la distancia al objetivo de ataque para comenzar a atacar si está lo suficientemente cerca
                Vector3 offset = attackController.targetToAttack.position - animator.transform.position;
                float sqrLen = offset.sqrMagnitude;
                if (sqrLen < attackingDistance * attackingDistance)
                {
                    agent.ResetPath(); // Detener el movimiento de la unidad
                    animator.SetBool("isAttacking", true);
                }
            }
        }
    }
}
