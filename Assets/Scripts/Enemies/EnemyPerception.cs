using UnityEngine;

public class EnemyPerception : MonoBehaviour
{
    [Header("Detection Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Edge Check Settings")]
    [Tooltip("How far forward from the enemy's center the sensor sits")]
    [SerializeField] private float edgeCheckForwardOffset = 0.3f;
    [Tooltip("How far down from the enemy's center the sensor starts")]
    [SerializeField] private float edgeCheckDownOffset = 0.3f;
    [SerializeField] private float edgeCheckDistance = 0.3f;

    [Header("Obstacle & Enemy Check Settings")]
    [Tooltip("How far forward to look for walls, higher ground, or other enemies")]
    [SerializeField] private float obstacleCheckDistance = 0.6f;
    [Tooltip("Vertical offset from center to cast the forward check (near feet to detect steps)")]
    [SerializeField] private float obstacleCheckHeight = -0.1f;

    [Header("Detection Area")]
    [Tooltip("Total radius the enemy can detect the player in all directions")]
    [SerializeField] private float detectionRadius = 8f;

    private bool isPlayerInRange;
    private Transform currentTarget;

    float facingDirection => Mathf.Sign(transform.localScale.x);

    private void Awake()
    {
        // Auto-assign enemyLayer if not explicitly set in Inspector
        if (enemyLayer == 0)
            enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
    }

    public bool HasGroundAhead()
    {
        Vector2 frontOrigin = (Vector2)transform.position +
            new Vector2(edgeCheckForwardOffset * facingDirection, -edgeCheckDownOffset);

        RaycastHit2D hit = Physics2D.Raycast(frontOrigin, Vector2.down, edgeCheckDistance, groundLayer);
        Debug.DrawRay(frontOrigin, Vector2.down * edgeCheckDistance, hit.collider != null ? Color.green : Color.red);

        return hit.collider != null;
    }

    // Detects walls or steps / higher ground in front
    public bool HasWallOrHigherGroundAhead()
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(0.2f * facingDirection, obstacleCheckHeight);
        Vector2 direction = Vector2.right * facingDirection;

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, obstacleCheckDistance, groundLayer);
        Debug.DrawRay(origin, direction * obstacleCheckDistance, Color.cyan);

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.transform.root != transform.root)
            {
                return true;
            }
        }
        return false;
    }

    // Detects other enemies ahead (ignoring himself and his own child colliders)
    public bool HasOtherEnemyAhead()
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(0.2f * facingDirection, obstacleCheckHeight);
        Vector2 direction = Vector2.right * facingDirection;

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, obstacleCheckDistance, enemyLayer);
        Debug.DrawRay(origin, direction * obstacleCheckDistance, Color.magenta);

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.transform.root != transform.root)
            {
                return true;
            }
        }
        return false;
    }

    public bool HasObstacleOrEnemyAhead()
    {
        return HasWallOrHigherGroundAhead() || HasOtherEnemyAhead();
    }

    public bool TryFindPlayer(out Transform playerTransform)
    {
        playerTransform = null;
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
        if (playerCollider == null)
            return false;

        Vector2 origin = transform.position;
        Vector2 playerPos = playerCollider.transform.position;
        Vector2 direction = (playerPos - origin).normalized;
        float distance = Vector2.Distance(origin, playerPos);

        RaycastHit2D obstacleHit = Physics2D.Raycast(origin, direction, distance, groundLayer);
        if (obstacleHit.collider != null)
        {
            return false;
        }

        playerTransform = playerCollider.transform;
        currentTarget = playerCollider.transform;
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isPlayerInRange ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}