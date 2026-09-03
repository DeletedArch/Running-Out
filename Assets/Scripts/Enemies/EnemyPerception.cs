using UnityEngine;

public class EnemyPerception : MonoBehaviour
{
    [Header("Detection Layers")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask playerLayer;

    [Header("Edge Check Settings")]
    [Tooltip("How far forward from the enemy's center the sensor sits")]
    [SerializeField] float edgeCheckForwardOffset = .3f;
    [Tooltip("How far down from the enemy's center the sensor starts")]
    [SerializeField] float edgeCheckDownOffset = .3f;
    [Tooltip("How long the downward laser is. Usually a small number like 0.3f to 0.5f" +
             "just long enough to touch the ground beneath the feet.")]
    [SerializeField] float edgeCheckDistance = .3f;

    [Header("Detection Area")]
    [Tooltip("Total radius the enemy can detect the player in all directions")]
    [SerializeField] private float detectionRadius = 8f;

    private bool isPlayerInRange;
    private Transform currentTarget;

    float facingDirection => Mathf.Sign(transform.localScale.x);

    public bool HasGroundAhead()
    {
        Vector2 FrontOrigin = (Vector2)transform.position +
            new Vector2(edgeCheckForwardOffset * facingDirection, -edgeCheckDownOffset);

        RaycastHit2D hit = Physics2D.Raycast(FrontOrigin, Vector2.down, edgeCheckDistance, groundLayer);
        Debug.DrawRay(FrontOrigin, Vector2.down * edgeCheckDistance, hit.collider != null ? Color.green : Color.red);

        return hit.collider != null;
    }



    public bool TryFindPlayer(out Transform playerTransform)
    {
        playerTransform = null;
        // 1. Is the player inside the detection circle?
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
        if (playerCollider == null)
            return false;

        // 2. Is there a wall or platform between enemy and player?
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
    //private void Update()
    //{
    //    isPlayerInRange = TryFindPlayer(out Transform playerTransform);
    //    HasGroundAhead();
    //}
    private void OnDrawGizmosSelected()
    {
        Vector3 enemyDetection = this.transform.position;
        if (isPlayerInRange)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.yellow;
        }
        Gizmos.DrawWireSphere(enemyDetection, detectionRadius);

    }
}