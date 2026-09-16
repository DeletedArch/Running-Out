using UnityEngine;

public class OnEdgeResponse : MonoBehaviour, IEdgeResponse
{
    [SerializeField] private float highJumpForce = 12f;
    [SerializeField] private float highJumpForwardSpeed = 3.2f;
    [SerializeField] private float highJumpHeightClearance = 2.0f;
    [SerializeField] private float gapJumpHeight = 1.5f;
    [SerializeField] private float dropJumpHeight = 0.4f;
    [SerializeField] private float dropJumpForwardMultiplier = 0.8f;

    private EnemyController enemyController;
    private EnemyPerception perception;
    private Rigidbody2D rb;

    private void Awake()
    {
        if (enemyController == null) enemyController = GetComponent<EnemyController>();
        if (perception == null) perception = GetComponent<EnemyPerception>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void HandleChaseEdge(Transform targetGround, float chaseSpeed)
    {
        if (targetGround == null || enemyController.EnemyMovement == null)
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
            float gapDistance = 1.5f; // Fallback distance
            if (perception.TryGetTargetPlatform(transform, out Bounds ownPlatform) &&
                perception.TryGetTargetPlatform(targetGround, out Bounds targetPlatform))
            {
                gapDistance = enemyController.EnemyMovement.FacingDirection > 0
                    ? Mathf.Max(0.5f, targetPlatform.min.x - ownPlatform.max.x)
                    : Mathf.Max(0.5f, ownPlatform.min.x - targetPlatform.max.x);
            }
            jumpPower = Mathf.Sqrt(2f * gravity * dropJumpHeight);
            // Time of flight: launch with jumpPower, dropping |diffY| down to the lower platform
            float airTime = (jumpPower + Mathf.Sqrt(jumpPower * jumpPower + 2f * gravity * Mathf.Abs(diffY))) / gravity;

            // Derive required forward velocity: distance / time + safety clearance
            forwardSpeed = (gapDistance + 0.5f) / Mathf.Max(0.1f, airTime);
        }
        else
        {
            jumpPower = Mathf.Sqrt(2f * gravity * gapJumpHeight);
            forwardSpeed = chaseSpeed;
        }

        enemyController.EnemyMovement.Jump(forwardSpeed, jumpPower);
    }

}