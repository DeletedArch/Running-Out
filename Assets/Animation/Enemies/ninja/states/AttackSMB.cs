using UnityEngine;

public class AttackSMB : EnemyStateBehaviour
{
    //do not use the parent's version for OnStateEnter use the new one i have here
    [SerializeField] private float attackRange = 1.3f;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //but we will still use the context from the ESB
        base.OnStateEnter(animator, stateInfo, layerIndex);
        Context.enemyMovement.Stop();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Context.perception.TryFindPlayer(out Transform playerTransform))
        {
            float distance = Vector2.Distance(Context.transform.position, playerTransform.position);
            if (distance > attackRange)
            {
                animator.SetBool("Attack", false);
            }
        }
        else
        {
            animator.SetBool("Attack", false);
            animator.SetBool("PlayerSpotted", false);
        }
    }

}