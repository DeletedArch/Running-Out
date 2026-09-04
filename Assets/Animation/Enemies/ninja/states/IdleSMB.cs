using UnityEngine;  

public class IdleSMB : EnemyStateBehaviour
{
    [Header("Patrol Pause")]
    [Tooltip("how long the enemy wait before he turns around")]
    [SerializeField] private float patrolIdleDuration = 1.5f;

    float timer;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        timer = 0;
        if (Context != null)
            Context.enemyMovement.Stop();
        
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Context == null) return;
        // Check if player is nearby
        if (Context.perception.TryFindPlayer(out Transform playerTransform)) {
            animator.SetBool("PlayerSpotted", true);
            return;
        }
        timer += Time.deltaTime;

        // return to patrol
        if (timer > patrolIdleDuration) {
            animator.SetTrigger("ResumePatrol");
        }
    }
}
