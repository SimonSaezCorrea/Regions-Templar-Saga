using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;

public class UnitAttackState : StateMachineBehaviour
{
    NavMeshAgent agent; // Componente NavMeshAgent para mover la unidad
    AttackController attackController; // Referencia al controlador de ataque de la unidad

    public float stopAttackingDistance = 1.2f; // Distancia a la que la unidad dejará de atacar

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent = animator.GetComponent<NavMeshAgent>(); // Obtener el componente NavMeshAgent de la unidad para poder moverla
        if (animator.TryGetComponent<AttackController>(out attackController))
        {
            attackController.SetAttackMaterial();
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (attackController == null) return;


        if(attackController.targetToAttack != null && animator.transform.GetComponent<UnitMovement>().isCommandedToMove == false)
        {
            LookAtTarjet();

            agent.SetDestination(attackController.targetToAttack.position); // Mover la unidad hacia el objetivo


            // Verificar la distancia al objetivo de ataque para dejar de atacar si está demasiado lejos
            Vector3 offset = attackController.targetToAttack.position - animator.transform.position;
            float sqrLen = offset.sqrMagnitude;
            if (sqrLen > stopAttackingDistance * stopAttackingDistance)
            {
                animator.SetBool("isAttacking", false);
            }

        }
        // Si el objetivo murió o desapareció
        else if (attackController.targetToAttack == null)
        {
            animator.SetBool("isAttacking", false);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    // Hace que la unidad mire hacia su objetivo de ataque
    private void LookAtTarjet()
    {
        if (attackController.targetToAttack == null) return;


        Vector3 direction = attackController.targetToAttack.position - agent.transform.position;
        // Evitar rotaciones si estamos encima del objetivo
        if (direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            agent.transform.rotation = Quaternion.Euler(0, lookRot.eulerAngles.y, 0);
        }
    }
}
