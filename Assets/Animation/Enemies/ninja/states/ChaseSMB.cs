using UnityEngine;

public class ChaseSMB : EnemyStateBehaviour
{
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float attackRange = 1.5f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

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

        Context.enemyMovement.FaceTarget(target.position);
        float distance = Vector2.Distance(Controller.transform.position, target.position);

        // --- Edge Detection ---
        bool dropAhead = !Context.perception.HasGroundAhead();
        bool wallAhead = Context.perception.HasWallOrHigherGroundAhead();
        bool playerIsHigher = target.position.y > Controller.transform.position.y + 0.3f;

        // ONLY jump if at a cliff edge, OR facing a higher tile wall while player is higher
        if (dropAhead || (wallAhead && playerIsHigher))
        {
            animator.SetTrigger("isJump");
            return;
        }

        // --- Ground Chase / Attack ---
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
        // Empty to preserve forward velocity into Jump and prevent trigger crash
    }
}