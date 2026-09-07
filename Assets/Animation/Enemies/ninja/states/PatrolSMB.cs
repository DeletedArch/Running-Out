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
        if (Context.channel != null && Context.channel.GetBestTarget(0f) != null)
        {
            animator.SetBool("PlayerSpotted", true);
            return;
        }

        // 2. Check for edge / cliff (DOES return to idle to pause at the ledge)
        if (!Context.perception.HasGroundAhead())
        {
            if (Context.edgeResponse != null)
            {
                Context.edgeResponse.OnEdgeDetected();
            }
            return;
        }

        // 3. No edge, but detected a wall, higher ground, or another enemy (DOES NOT return to idle, turns around immediately)
        if (turnCooldownTimer <= 0f && Context.perception.HasObstacleOrEnemyAhead())
        {
            Context.enemyMovement.Flip();
            turnCooldownTimer = turnCooldown;
            return;
        }

        // 4. Move forward in the direction he is facing
        Context.enemyMovement.Move(patrolSpeed);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Context != null)
            Context.enemyMovement.Stop();
    }
}