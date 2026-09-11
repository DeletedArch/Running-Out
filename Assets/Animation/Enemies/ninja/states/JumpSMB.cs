using UnityEngine;

public class JumpSMB : EnemyStateBehaviour
{
    private Rigidbody2D rb;
    private float takeoffTimer;
    private const float TakeoffGraceDuration = 0.2f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (Controller == null) return;

        rb = Controller.GetComponent<Rigidbody2D>();
        takeoffTimer = TakeoffGraceDuration;

        Transform target = Context.Target ?? Context.perception.CurrentTarget;
        var edge = Context.edgeResponse ?? Controller.GetComponent<IEdgeResponse>();

        if (edge != null && target != null)
        {
            edge.HandleChaseEdge(target, 5f);
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Controller == null || Context == null || Context.enemyMovement == null || rb == null)
            return;

        // Grace period: do not check for landing while launching off the ground
        if (takeoffTimer > 0f)
        {
            takeoffTimer -= Time.deltaTime;
            return;
        }

        // Land ONLY when falling downward (velocity.y <= 0) and touching ground
        if (rb.linearVelocity.y <= 0.05f && Context.enemyMovement.IsGrounded())
        {
            animator.Play("chase");
        }
    }
}