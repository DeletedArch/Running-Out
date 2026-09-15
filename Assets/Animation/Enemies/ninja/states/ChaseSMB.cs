using Unity.VisualScripting;
using UnityEngine;

public class ChaseSMB : EnemyStateBehaviour
{
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float takeoffOffset = 2.0f;
    [SerializeField] private float jumpCooldown = 0.8f;
    [Header("attack data")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.2f;
    private float attackCooldownTimer = 0f;
    private float jumpCooldownTimer = 0f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        jumpCooldownTimer = jumpCooldown;
        attackCooldownTimer = 0f; // Can attack immediately upon reaching player
        if (Context != null && Context.Target == null && Context.perception != null)
        {
            if (Context.perception.TryFindPlayer(out Transform found))
            {
                Context.Target = found;
            }
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Context == null || Context.perception == null) return;

        if (jumpCooldownTimer > 0f)
            jumpCooldownTimer -= Time.deltaTime;

        // Count down attack cooldown
        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;

        Transform target = Context.Target ?? Context.perception.CurrentTarget;
        if (target == null)
        {
            if (Context.perception.TryFindPlayer(out target))
            {
                Context.Target = target;
            }
            else
            {
                Context.enemyMovement.Move(chaseSpeed);
                return;
            }
        }

        float distance = Vector2.Distance(Controller.transform.position, target.position);
        // If the player is within sword range and at similar height:
        if (distance <= attackRange && Mathf.Abs(target.position.y - Controller.transform.position.y) < 1.0f)
        {
            Context.enemyMovement.Stop();
            Context.enemyMovement.FaceTarget(target.position);
            if (attackCooldownTimer <= 0f)
            {
                attackCooldownTimer = attackCooldown;
                animator.SetTrigger("Attack");
            }
            return;
        }

        bool playerIsHigher = target.position.y > Controller.transform.position.y + 0.6f;
        if (playerIsHigher)
        {
            if (Context.perception.TryGetTargetPlatform(target, out Bounds platformBounds))
            {
                float enemyX = Controller.transform.position.x;
                float leftTakeoff = platformBounds.min.x - takeoffOffset;
                float rightTakeoff = platformBounds.max.x + takeoffOffset;
                float chosenTakeoffX;
                if (enemyX < platformBounds.min.x)
                    chosenTakeoffX = leftTakeoff;
                else if (enemyX > platformBounds.max.x)
                    chosenTakeoffX = rightTakeoff;
                else
                {
                    float distToLeft = Mathf.Abs(enemyX - leftTakeoff);
                    float distToRight = Mathf.Abs(enemyX - rightTakeoff);
                    chosenTakeoffX = distToLeft < distToRight ? leftTakeoff : rightTakeoff;
                }
                bool hasCeiling = Context.perception.HasCeilingAbove(3.0f);
                float distToTakeoff = Mathf.Abs(enemyX - chosenTakeoffX);
                if (distToTakeoff > 0.5f || hasCeiling)
                {
                    Vector2 runTarget = new Vector2(chosenTakeoffX, Controller.transform.position.y);
                    Context.enemyMovement.FaceTarget(runTarget);
                    Context.enemyMovement.Move(chaseSpeed);
                    return;
                }
                else
                {
                    if (jumpCooldownTimer <= 0f)
                    {
                        jumpCooldownTimer = jumpCooldown;
                        Context.enemyMovement.FaceTarget(platformBounds.center);
                        animator.SetTrigger("isJump");
                    }
                    return;
                }
            }
        }

        // 1. Ally check: Stop if another enemy is directly in front
        if (Context.perception.HasOtherEnemyAhead())
        {
            Context.enemyMovement.Stop();
            return;
        }

        // 2. Check if player is lower than the enemy
        bool playerIsLower = target.position.y < Controller.transform.position.y - 0.5f;
        if(playerIsLower){
            float gap = 0f;
            bool haveOwn = Context.perception.TryGetTargetPlatform(Controller.transform, out Bounds ownPlatform);
            bool haveTarget = Context.perception.TryGetTargetPlatform(target, out Bounds targetPlatform);

            if (haveTarget && haveOwn) {
                gap = Context.enemyMovement.FacingDirection > 0 ?
                    targetPlatform.min.x - ownPlatform.max.x : ownPlatform.min.x - targetPlatform.max.x;
            }

            const float stepThreshold = 0.6f;
            if (gap <= stepThreshold)
            {
                Context.enemyMovement.FaceTarget(target.position);
                Context.enemyMovement.Move(chaseSpeed);
                return;
            }

            if (jumpCooldownTimer <= 0f)
            {
                jumpCooldownTimer = jumpCooldown;
                Context.enemyMovement.FaceTarget(target.position);
                animator.SetTrigger("isJump");
            }
            return;
        }

        // 3. Level Ground Chase:
        Context.enemyMovement.FaceTarget(target.position);
        bool dropAhead = !Context.perception.HasGroundAhead();
        bool wallAhead = Context.perception.HasWallOrHigherGroundAhead();
        // If wall is too tall to clear, don't jump into it endlessly
        if (wallAhead && !Context.perception.CanHopObstacle(1.6f))
        {
            Context.enemyMovement.Stop();
            return;
        }
        if (jumpCooldownTimer <= 0f && (dropAhead || wallAhead))
        {
            jumpCooldownTimer = jumpCooldown;
            animator.SetTrigger("isJump");
            return;
        }

        Context.enemyMovement.Move(chaseSpeed);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }
}