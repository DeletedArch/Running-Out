using UnityEngine;


public class ChaseSMB : EnemyStateBehaviour
{
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float attackRange = 1.3f;

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Context == null) return;
        // can you see the player when you are in your path
        if (Context.perception.TryFindPlayer(out Transform playerTransform) && Context.perception.HasGroundAhead())
        {
            float distance = Vector2.Distance(Context.transform.position,playerTransform.position );
            if (distance < attackRange)
            {
                Context.enemyMovement.Stop();
                animator.SetBool("Attack", true);
            }
            else
            {
                if (!Context.perception.HasGroundAhead())
                    Context.edgeResponse.OnEdgeDetected();

                Context.enemyMovement.FaceTarget(playerTransform.position);
                Context.enemyMovement.Move(chaseSpeed);
            }
        }
        else
        {
            animator.SetBool("PlayerSpotted", false);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Context != null)
        {
            Context.enemyMovement.Stop();
        }
    }
}
