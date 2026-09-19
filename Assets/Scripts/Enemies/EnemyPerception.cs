using UnityEngine;

public class EnemyPerception : MonoBehaviour
{
    private EnemyContext context;
    private LayerMask groundLayer => context != null ? context.groundLayer : default;
    private LayerMask playerLayer => context != null ? context.playerLayer : default;
    private LayerMask enemyLayer => context != null ? context.enemyLayer : default;

    [Header("Edge Check Settings")]
    [SerializeField] private float edgeCheckForwardOffset = 0.3f;
    [SerializeField] private float edgeCheckDownOffset = 0.3f;
    [SerializeField] private float edgeCheckDistance = 0.3f;

    [Header("Obstacle & Enemy Check Settings")]
    [SerializeField] private float obstacleCheckDistance = 0.6f;
    [SerializeField] private float obstacleCheckHeight = -0.1f;

    [Header("Detection Area")]
    [SerializeField] private float detectionRadius = 8f;

    private Transform currentTarget;
    public Transform CurrentTarget => currentTarget;

    float facingDirection => transform.localScale.x < 0 ? 1f : -1f;

    private void Awake()
    {
        var controller = GetComponent<EnemyController>();
        if (controller != null)
        {
            context = controller.Context;
        }
    }

    public bool HasGroundAhead()
    {
        Vector2 frontOrigin = (Vector2)transform.position +
            new Vector2(edgeCheckForwardOffset * facingDirection, - edgeCheckDownOffset);

        RaycastHit2D hit = Physics2D.Raycast(frontOrigin, Vector2.down, edgeCheckDistance, groundLayer);
        Debug.DrawRay(frontOrigin, Vector2.down * edgeCheckDistance, hit.collider != null ? Color.green : Color.red);

        return hit.collider != null;
    }
    public bool HasGroundBehind()
    {
        // Notice the minus sign (-) before edgeCheckForwardOffset:
        // This shoots the ray behind him instead of in front of him!
        Vector2 backOrigin = (Vector2)transform.position +
            new Vector2(-edgeCheckForwardOffset * facingDirection, -edgeCheckDownOffset);
        RaycastHit2D hit = Physics2D.Raycast(backOrigin, Vector2.down, edgeCheckDistance, groundLayer);
        Debug.DrawRay(backOrigin, Vector2.down * edgeCheckDistance, hit.collider != null ? Color.green : Color.red);
        return hit.collider != null;
    }

    public bool HasWallOrHigherGroundAhead()
    {
        // Center the check at torso height (Y = 0.5f)
        Vector2 boxCenter = (Vector2)transform.position + new Vector2(0.4f * facingDirection, 0.5f);
        Vector2 boxSize = new Vector2(0.2f, 1.2f); // Width and full body height of the check
        Vector2 direction = Vector2.right * facingDirection;
        float checkDistance = 0.5f;
        int mask = (1 << LayerMask.NameToLayer("Ground")) |
                   (1 << LayerMask.NameToLayer("Wall"));
        RaycastHit2D hit = Physics2D.BoxCast(boxCenter, boxSize, 0f, direction, checkDistance, mask);

        // Visual debug box in Scene view
        Debug.DrawRay(boxCenter, direction * checkDistance, hit.collider != null ? Color.red : Color.cyan);
        if (hit.collider != null && hit.collider.transform.root != transform.root && !hit.collider.isTrigger)
        {
            return true;
        }
        return false;
    }

    public bool HasOtherEnemyAhead()
    {
        // 1. Box in front of the enemy at chest height (Y = 0.4f)
        Vector2 checkCenter = (Vector2)transform.position + new Vector2(0.8f * facingDirection, 0.4f);
        Vector2 checkSize = new Vector2(0.9f, 1.2f);
        int mask = (1 << LayerMask.NameToLayer("Enemy")) | (1 << 0);
        if (enemyLayer.value != 0) mask |= enemyLayer.value;
        Collider2D[] colliders = Physics2D.OverlapBoxAll(checkCenter, checkSize, 0f, mask);
        foreach (var col in colliders)
        {
            // 2. Only ignore colliders that belong to THIS enemy (sword, self, triggers)
            // Even if enemies share the 'ninja enemies' folder, this correctly detects other enemies!
            if (col == null || col.transform.IsChildOf(transform) || col.isTrigger)
                continue;
            // 3. Detect other enemies by Tag or Layer:
            if (col.CompareTag("Enemy") || col.gameObject.layer == LayerMask.NameToLayer("Enemy"))
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

        // 1. Line-of-sight check: cast a line from enemy eye to player center
        Vector2 eyePosition = (Vector2)transform.position + Vector2.up * 0.2f;
        Vector2 playerCenter = playerCollider.bounds.center;
        int obstacleMask = (1 << LayerMask.NameToLayer("Ground")) | (1 << LayerMask.NameToLayer("Wall"));
        RaycastHit2D blockHit = Physics2D.Linecast(eyePosition, playerCenter, obstacleMask);
        if (blockHit.collider != null)
        {
            // Line of sight is blocked by a wall or floor!
            Debug.DrawLine(eyePosition, blockHit.point, Color.red);
            return false;
        }
        // Line of sight is clear:
        Debug.DrawLine(eyePosition, playerCenter, Color.green);
        playerTransform = playerCollider.transform;
        currentTarget = playerCollider.transform;
        return true;
    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = currentTarget != null ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    // if the enemy is upper
    public bool CanHopObstacle(float maxHopHeight = 1.6f)
    {
        Vector2 highOrigin = (Vector2)transform.position + new Vector2(0.2f * facingDirection, maxHopHeight);
        RaycastHit2D highHit = Physics2D.Raycast(highOrigin, Vector2.right * facingDirection, obstacleCheckDistance, groundLayer);
        Debug.DrawRay(highOrigin, Vector2.right * facingDirection * obstacleCheckDistance, highHit.collider == null ? Color.green : Color.red);
        return highHit.collider == null; // Returns true if the space above is clear
    }

    // if the enemy is lower
    public bool HasCeilingAbove(float checkDistance = 3.0f)
    {
        Vector2 origin = (Vector2)transform.position + Vector2.up * 2f;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up, checkDistance, groundLayer);
        Debug.DrawRay(origin, Vector2.up * checkDistance, hit.collider != null ? Color.red : Color.green);
        return hit.collider != null;
    }

    public bool TryGetTargetPlatform(Transform targetTransform, out Bounds platformBounds)
    {
        platformBounds = default;
        if (targetTransform == null) return false;
        RaycastHit2D hit = Physics2D.Raycast(targetTransform.position, Vector2.down, 3.5f, groundLayer);
        if (hit.collider != null)
        {
            platformBounds = hit.collider.bounds;
            return true;
        }
        return false;
    }
}