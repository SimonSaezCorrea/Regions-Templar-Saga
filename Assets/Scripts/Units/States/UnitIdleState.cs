using UnityEngine;

public class UnitIdleState : StateMachineBehaviour
{

    AttackController attackController; // Referencia al controlador de ataque de la unidad

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateinfo, int layerindex)
    {
        attackController = animator.transform.GetComponent<AttackController>(); // Obtener el componente AttackController de la unidad
        attackController.SetIdleMaterial();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Verificar si hay un objetivo para atacar
        if (attackController.targetToAttack != null)
        {
            // Transición a estado de seguimiento
            animator.SetBool("isFollowing", true);

            // Aquí podría ir cualquier sonido o efecto visual para indicar que la unidad ha detectado un objetivo
            // Por ejemplo un sonido que diga "Go, go, go"
        }
    }
}
