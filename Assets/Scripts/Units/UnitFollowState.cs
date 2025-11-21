using UnityEngine;
using UnityEngine.AI;

public class UnitFollowState : StateMachineBehaviour
{

    AttackController attackController;
    NavMeshAgent agent;

    public float attackingDistance = 1f;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        attackController = animator.transform.GetComponent<AttackController>();
        agent = animator.transform.GetComponent<NavMeshAgent>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(attackController.tarjetToAttack == null)
        {
            animator.SetBool("isFollowing", false);
        }
        else
        {
            if (!animator.transform.GetComponent<UnitMovement>().isCommandedToMove)
            {
                agent.SetDestination(attackController.tarjetToAttack.position);
                animator.transform.LookAt(attackController.tarjetToAttack);

                //float distanceFromTarget = Vector3.Distance(attackController.tarjetToAttack.position, animator.transform.position);
                //if(distanceFromTarget < attackingDistance)
                //{
                //    animator.SetBool("isAttacking", true);
                //}
            }
        }
    }


    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent.SetDestination(animator.transform.position);
    }
}
