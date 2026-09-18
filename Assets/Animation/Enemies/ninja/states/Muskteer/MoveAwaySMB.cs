using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class MoveAwaySMB : EnemyStateBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float safeDistance = 6.5f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        animator.SetBool("MoveDistance", true);

        bool hasGroundBehind = Context.perception != null && Context.perception.HasGroundBehind();
        Transform target = Context.Target ?? Context.perception?.CurrentTarget;

        if (!hasGroundBehind)
        {
            Context.enemyMovement.Stop();
            if (target != null)
            {
                Context.enemyMovement.FaceTarget(target.position);
            }

            // Satisfies your Animator transition: move away -> Aim
            animator.SetBool("MoveDistance", false);
            animator.SetBool("PlayerClose", false);
            return;
        }

        // locate where is the player to get away from him (front / back)
        if (target == null && Context.perception != null)
        {
            if(Context.perception.TryFindPlayer(out Transform found))
            {
                Context.Target = found;
                target = found;
            }
        }

        // face away from him
        if (target != null) 
            Context.enemyMovement.FaceAwayFromTarget(target.position);
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        bool atEdge = Context.perception != null && !Context.perception.HasGroundAhead();
        bool atWall = Context.perception != null && Context.perception.HasObstacleOrEnemyAhead();

        Transform target = Context.Target ?? Context.perception?.CurrentTarget;

        if (target == null) {
            Context.enemyMovement.Stop();
            // Satisfies your Animator transition: move away -> Aim
            animator.SetBool("MoveDistance", false);
            animator.SetBool("PlayerClose", false);
        }

        if (atEdge || atWall)
        {
            Context.enemyMovement.Stop();
            if (target != null)
            {
                Context.enemyMovement.FaceTarget(target.position);
            }

            // Satisfies your Animator transition: move away -> Aim
            animator.SetBool("MoveDistance", false);
            animator.SetBool("PlayerClose", false);
            return;
        }

        float distanceToPlayer = Vector2.Distance(Controller.transform.position, target.position);
        if (distanceToPlayer >= safeDistance)
        {
            // Far enough. Stop and aim.
            Context.enemyMovement.Stop();
            if (target != null)
            {
                Context.enemyMovement.FaceTarget(target.position);
            }

            // Satisfies your Animator transition: move away -> Aim
            animator.SetBool("PlayerClose", false);
            animator.SetBool("MoveDistance", false);
            return;
        }

        // run away (the action)
        Context.enemyMovement.FaceAwayFromTarget(target.position);
        Context.enemyMovement.Move(moveSpeed);
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        // clean up velocity
        Context?.enemyMovement?.Stop();

        Transform target = Context?.Target ?? Context?.perception?.CurrentTarget;
        //if (target != null && Context?.enemyMovement != null)
        //{
            Context.enemyMovement.FaceTarget(target.position);
        //}
    }

}
