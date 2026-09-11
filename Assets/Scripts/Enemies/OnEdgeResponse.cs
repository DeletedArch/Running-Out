using UnityEngine;

public class OnEdgeResponse : MonoBehaviour, IEdgeResponse
{
    [SerializeField] private float highJumpForce = 12f;
    [SerializeField] private float highJumpForwardSpeed = 3.2f;
    [SerializeField] private float highJumpHeightClearance = 2.0f;
    [SerializeField] private float gapJumpHeight = 1.5f;
    [SerializeField] private float dropJumpHeight = 0.4f;
    [SerializeField] private float dropJumpForwardMultiplier = 0.8f;

    private EnemyMovement movement;
    private EnemyPerception perception;
    private Rigidbody2D rb;

    private void Awake()
    {
        if (movement == null) movement = GetComponent<EnemyMovement>();
        if (perception == null) perception = GetComponent<EnemyPerception>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void HandleChaseEdge(Transform targetGround, float chaseSpeed)
    {
        if (targetGround == null || movement == null)
            return;

        float diffY = targetGround.position.y - transform.position.y;
        float gravity = Mathf.Abs(Physics2D.gravity.y * (rb != null ? rb.gravityScale : 1f));
        if (gravity <= 0.01f) gravity = 9.81f;

        float jumpPower;
        float forwardSpeed;

        if (diffY > 0.3f)
        {
            float calculatedPower = Mathf.Sqrt(2f * gravity * (diffY + highJumpHeightClearance));
            jumpPower = Mathf.Max(highJumpForce, calculatedPower);
            forwardSpeed = highJumpForwardSpeed;
        }
        else if (diffY < -0.5f)
        {
            jumpPower = Mathf.Sqrt(2f * gravity * dropJumpHeight);
            forwardSpeed = chaseSpeed * dropJumpForwardMultiplier;
        }
        else
        {
            jumpPower = Mathf.Sqrt(2f * gravity * gapJumpHeight);
            forwardSpeed = chaseSpeed;
        }

        movement.Jump(forwardSpeed, jumpPower);
    }

}