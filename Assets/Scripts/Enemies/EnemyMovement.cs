using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;

    // Directly derived from scale: 1 for facing right, -1 for facing left
    public int FacingDirection => transform.localScale.x < 0 ? -1 : 1;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    // Moves forward in the current facing direction at given speed
    public void Move(float speed)
    {
        if (rb != null)
            rb.linearVelocity = new Vector2(FacingDirection * speed, rb.linearVelocity.y);
    }

    // Moves in an explicit direction: +1 for Right, -1 for Left
    public void MovementDirection(int direction, float speed)
    {
        if (rb != null)
            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
    }

    // Pushes the enemy backward (opposite of facing direction)
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