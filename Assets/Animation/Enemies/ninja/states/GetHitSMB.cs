using UnityEngine;

public class GetHitSMB : EnemyStateBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (Context != null) Context.enemyMovement.Stop();
        
        animator.SetBool("isHit", false);
    }
}