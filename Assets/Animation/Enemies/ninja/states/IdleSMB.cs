using UnityEngine;  

public class IdleSMB : EnemyStateBehaviour
{
    [Header("Patrol Edge Pause")]
    [SerializeField] private float patrolIdleDuration = 1.5f;
    [Header("Combat Pause")]
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private float attackRange = 1.3f;
    private float timer;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        timer = 0f;
        if (Context != null) Context.enemyMovement.Stop();
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Context == null) return;
        timer += Time.deltaTime;
        // --- COMBAT CHECK ---
        if (Context.perception.TryFindPlayer(out Transform playerTransform))
        {
            animator.SetBool("PlayerSpotted", true);
            Context.enemyMovement.FaceTarget(playerTransform.position);
            float distance = Vector2.Distance(Controller.transform.position, playerTransform.position);
            if (distance <= attackRange)
            {
                if (timer >= attackCooldown)
                {
                    animator.SetBool("Attack", true);
                    animator.SetBool("isIdle", false);
                }
            }
            else
            {
                animator.SetBool("isIdle", false);
            }
            return;
        }
        // --- PATROL EDGE CHECK ---
        if (timer >= patrolIdleDuration)
        {
            Context.enemyMovement.Flip();
            animator.SetBool("isIdle", false); 
        }
    }
}
