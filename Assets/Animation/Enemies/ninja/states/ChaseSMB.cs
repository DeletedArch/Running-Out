using UnityEngine;

public class ChaseSMB : EnemyStateBehaviour
{
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float takeoffOffset = 2.0f;
    [SerializeField] private float jumpCooldown = 0.8f;
    private float jumpCooldownTimer = 0f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        jumpCooldownTimer = jumpCooldown;

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

        Context.enemyMovement.FaceTarget(target.position);

        bool dropAhead = !Context.perception.HasGroundAhead();
        bool wallAhead = Context.perception.HasWallOrHigherGroundAhead();

        if (jumpCooldownTimer <= 0f && (dropAhead || wallAhead))
        {
            jumpCooldownTimer = jumpCooldown;
            animator.SetTrigger("isJump");
            return;
        }

        if (distance <= attackRange)
        {
            Context.enemyMovement.Stop();
            animator.SetTrigger("Attack");
        }
        else
        {
            Context.enemyMovement.Move(chaseSpeed);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }
}