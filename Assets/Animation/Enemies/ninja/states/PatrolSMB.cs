using UnityEngine;

public class PatrolSMB : EnemyStateBehaviour
{
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float turnCooldown = 0.25f;
    private float turnCooldownTimer = 0f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        turnCooldownTimer = 0f;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Context == null)
            return;

        if (turnCooldownTimer > 0f)
            turnCooldownTimer -= Time.deltaTime;

        // 1. Look for player
        if (Context.perception != null && Context.perception.TryFindPlayer(out Transform target))
        {
            Context.Target = target; // Store in context for Chase & Jump
            animator.SetBool("PlayerSpotted", true);
            return;
        }

        bool atEdge = !Context.perception.HasGroundAhead();
        bool atWall = Context.perception.HasObstacleOrEnemyAhead();

        // 2. No edge, but detected a wall, higher ground, or another enemy (DOES NOT return to idle, turns around immediately)
        if ((atEdge || atWall) && turnCooldownTimer <= 0f)
        {
            Context.enemyMovement.Flip();
            turnCooldownTimer = turnCooldown;
            return;
        }

        // 3. Move forward in the direction he is facing
        Context.enemyMovement.Move(patrolSpeed);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Context != null)
            Context.enemyMovement.Stop();
    }
}