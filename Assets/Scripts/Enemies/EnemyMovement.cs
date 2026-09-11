using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private EnemyContext context;
    private Rigidbody2D rb => context != null ? context.rb : null;
    private LayerMask groundLayer => context != null ? context.groundLayer : default;

    public int FacingDirection => transform.localScale.x < 0 ? -1 : 1;

    private void Awake()
    {
        var controller = GetComponent<EnemyController>();
        if (controller != null)
        {
            context = controller.Context;
        }
    }

    public void Move(float speed)
    {
        if (rb != null)
            rb.linearVelocity = new Vector2(FacingDirection * speed, rb.linearVelocity.y);
    }

    public bool IsGrounded()
    {
        Vector2 footPos = (Vector2)transform.position + new Vector2(0, -0.5f);
        return Physics2D.OverlapBox(footPos, new Vector2(0.5f, 0.5f), 0f, groundLayer) != null;
    }

    public void Jump(float forwardSpeed, float jumpPower)
    {
        if (rb != null)
            rb.linearVelocity = new Vector2(forwardSpeed * FacingDirection, jumpPower);
    }

    public void StepBack(float force)
    {
        if (rb != null)
            rb.linearVelocity = new Vector2(-force * FacingDirection, rb.linearVelocity.y);
    }

    public void Flip()
    {
        transform.localScale = new Vector3(
            -transform.localScale.x,
            transform.localScale.y,
            transform.localScale.z
        );
    }

    public void FaceTarget(Vector2 targetPos)
    {
        float diffX = targetPos.x - transform.position.x;
        if (diffX < -0.05f && FacingDirection > 0)
            Flip();
        else if (diffX > 0.05f && FacingDirection < 0)
            Flip();
    }

    public void FaceAwayFromTarget(Vector2 targetPos)
    {
        float diffX = targetPos.x - transform.position.x;
        if (diffX < -0.05f && FacingDirection < 0)
            Flip();
        else if (diffX > 0.05f && FacingDirection > 0)
            Flip();
    }

    public void Stop()
    {
        if (rb != null)
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
}