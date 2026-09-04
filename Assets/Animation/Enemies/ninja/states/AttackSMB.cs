using UnityEngine;
using Cysharp.Threading.Tasks;

public class AttackSMB : EnemyStateBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        Context.enemyMovement.Stop();
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!animator.IsInTransition(layerIndex))
        {
            animator.SetBool("Attack", false);
        }
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("isIdle", true);
    }

    public UniTask ApplyDamage()
    {
        return UniTask.Delay(300).ContinueWith(() => 
        { 
            GameObject targetedPlayer = Context.channel.GetBestTarget(0f, 1f).Object;

            if (targetedPlayer != null)
            {
                Debug.Log("AttackImpulseSMB: Attempting to damage the enemy.");
                var damageable = targetedPlayer.GetComponent<IEntity>();
                if (damageable != null)
                {
                    Debug.Log("AttackImpulseSMB: Damaging the enemy.");
                    damageable.TakeDamage(.5f);
                }
            }
        });
    }
}