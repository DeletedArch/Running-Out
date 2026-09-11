using UnityEngine;

public class OnEdgeResponse : MonoBehaviour, IEdgeResponse
{
    [SerializeField] private EnemyMovement movement;
    [SerializeField] private EnemyPerception perception;
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

        if (diffY > 0.3f)
        {
            // Higher platform: calculate velocity needed to reach height difference + 0.8m clearance
            float requiredHeight = diffY + 0.8f;
            jumpPower = Mathf.Sqrt(2f * gravity * requiredHeight);
        }
        else if (diffY < -0.5f)
        {
            // Lower platform / drop: small hop forward
            jumpPower = Mathf.Sqrt(2f * gravity * 0.4f);
        }
        else
        {
            // Same level gap jump: standard arc
            jumpPower = Mathf.Sqrt(2f * gravity * 1.2f);
        }

        movement.Jump(chaseSpeed, jumpPower);
    }
}