using UnityEngine;

public class JumpSMB : EnemyStateBehaviour
{
    private Rigidbody2D rb;
    private float takeoffTimer;
    private float airTime;
    private const float TakeoffGraceDuration = 0.2f;
    private const float MaxAirTime = 1.2f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (Controller == null) return;

        animator.ResetTrigger("isJump");

        rb = Controller.GetComponent<Rigidbody2D>();
        takeoffTimer = TakeoffGraceDuration;
        airTime = 0f;

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

        airTime += Time.deltaTime;

        if (takeoffTimer > 0f)
        {
            takeoffTimer -= Time.deltaTime;
            return;
        }

        bool isFalling = rb.linearVelocity.y <= 0.1f;
        bool isGrounded = Context.enemyMovement.IsGrounded();
        bool timedOut = airTime >= MaxAirTime;

        if ((isFalling && isGrounded) || timedOut)
        {
            animator.ResetTrigger("isJump");
            animator.Play("chase");
        }
    }
}