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

    float facingDirection => Mathf.Sign(transform.localScale.x);

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
            new Vector2(edgeCheckForwardOffset * facingDirection, -edgeCheckDownOffset);

        RaycastHit2D hit = Physics2D.Raycast(frontOrigin, Vector2.down, edgeCheckDistance, groundLayer);
        Debug.DrawRay(frontOrigin, Vector2.down * edgeCheckDistance, hit.collider != null ? Color.green : Color.red);

        return hit.collider != null;
    }

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

        playerTransform = playerCollider.transform;
        currentTarget = playerCollider.transform;
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = currentTarget != null ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}