using UnityEngine;
using static UnityEngine.UI.Image;

//Movement Toolbox to control all the movements that enemies can do

/// <summary>
/// Move in a direction at a speed (Walk, Run, Chase, Retreat).
/// Apply a sudden burst/impulse (Step Back, Dodge, Knockback).
/// Face a direction (Flip, Face the player, Turn away from the player).
/// Stop (Idle, Stun).
/// </summary>

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    public int FacingDirection { get; private set; } = 1;
    private void Awake()
    {
        if(rb == null) 
            rb = GetComponent<Rigidbody2D>();
        FacingDirection = transform.localScale.x >= 0 ? 1 : -1;
    }

    // Moves forward in the current facing direction at any speed (walk, run, chase).
    public void Move(float speed)
    {
        if (rb != null)
            rb.linearVelocity = new Vector2 (FacingDirection * speed, rb.linearVelocity.y);
    }

    // Moves in an explicit direction: +1 for Right, -1 for Left.
    public void MovementDirection(int direction,float speed)
    {
        if (rb != null)
            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
    }

    // Pushes the enemy backward (opposite of facing direction).
    public void StepBack(float force)
    {
        if (rb != null)
            rb.linearVelocity = new Vector2( -force * FacingDirection, rb.linearVelocity.y);
    }

    public void Flip()
    {
        FacingDirection *= -1;
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x) * FacingDirection,
            transform.localScale.y, 
            transform.localScale.z
            );
    }

    public void FaceTarget( Vector2 targetPos)
    {
        Vector2 origin = transform.position;
        Vector2 direction = (targetPos - origin);
        float diffX = direction.x;
        if (diffX < 0 && FacingDirection > 0)
            Flip();

        else if (diffX > 0 && FacingDirection < 0)
            Flip();
    }

    public void FaceAwayFromTarget( Vector2 targetPos)
    {
        Vector2 origin = transform.position;
        Vector2 direction = (targetPos - origin);
        float diffX = direction.x;
        if (diffX < 0 && FacingDirection < 0)
            Flip();

        else if (diffX > 0 && FacingDirection > 0)
            Flip();
    }

    public void Stop()
    {
        if (rb != null)
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
}
