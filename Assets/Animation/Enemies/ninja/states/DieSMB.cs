using UnityEngine;

public class DieSMB : EnemyStateBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (Context != null)
        {
            Context.enemyMovement.Stop();
        }
        Destroy(animator.gameObject, 3f);
    }
}
