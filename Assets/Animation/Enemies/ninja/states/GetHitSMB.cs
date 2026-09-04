using UnityEngine;

public class GetHitSMB : EnemyStateBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        // Stop moving while taking the hit
        if (Context != null)
        {
            Context.enemyMovement.Stop();
        }
    }
}