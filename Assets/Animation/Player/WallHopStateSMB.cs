using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class WallHopStateSMB : StateMachineBehaviour
{
    [SerializeField] private float wallHopDuration = 0.15f;

    private CancellationTokenSource stateCts;
    private Rigidbody2D rb;
    private PlayerContext context;
    private PlayerController playerController;
    private float originalGravityScale = 1f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        stateCts?.Cancel();
        stateCts?.Dispose();
        stateCts = new CancellationTokenSource();

        playerController = animator.GetComponent<PlayerController>();
        PlayerContext context = playerController.Context;
        if (context != null)
        {
            Vector2 wallHopDirection = playerController.GetWallTouchDirection() == WallTouchDirection.Left ? Vector2.right : Vector2.left;
            this.context = context;
            rb = context.playerRigidbody;
            originalGravityScale = rb.gravityScale;
            rb.gravityScale = 0f;
            Vector2? nextWallPosition;
            if (animator.GetBool("IsGrounded"))
            {
                nextWallPosition = FindFirstWall(wallHopDirection);
                nextWallPosition += Vector2.up * context.playerMovementConfig.WallHopMaxDistance.y / 1.5f; // Add vertical offset to hop up
            } else
            {
                nextWallPosition = FindNextWall(wallHopDirection);
            }
            Vector2 finalHopPosition;
            if (nextWallPosition.HasValue)
            {
                finalHopPosition = nextWallPosition.Value;
            }
            else
            {
                finalHopPosition = rb.position + wallHopDirection * context.playerMovementConfig.WallHopMaxDistance.x + Vector2.up * context.playerMovementConfig.WallHopMaxDistance.y * 1.5f;
            }

            ApplyWallHop(rb, finalHopPosition, stateCts.Token).Forget();
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (rb == null || context == null) return;

        rb.linearVelocity = Vector2.zero;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        stateCts?.Cancel();
        stateCts?.Dispose();
        stateCts = null;

        if (rb != null)
        {
            rb.gravityScale = originalGravityScale;
        }
        animator.SetBool("WallHopping", false);
    }

    async UniTaskVoid ApplyWallHop(Rigidbody2D rb, Vector2 wallHopPosition, CancellationToken ct)
    {
        int direction = (int)playerController.GetWallTouchDirection();
        rb.transform.localScale = new Vector3(-direction * Mathf.Abs(rb.transform.localScale.x), rb.transform.localScale.y, rb.transform.localScale.z);
        Vector2 startPosition = rb.position;
        float elapsedTime = 0f;
        float newWallHopDuration = wallHopDuration * context.playerAnimator.GetFloat("Timer");
        try
        {
            await UniTask.WaitUntil(() =>
            {
                elapsedTime += Time.fixedDeltaTime;
                rb.MovePosition(Vector2.Lerp(startPosition, wallHopPosition, elapsedTime / newWallHopDuration));
                return elapsedTime >= newWallHopDuration;
            }, PlayerLoopTiming.FixedUpdate, ct);

            rb.gravityScale = originalGravityScale;
            context.playerAnimator.SetBool("WallHopping", false);
        }
        catch (System.OperationCanceledException)
        {
            // Interrupted early
        }
    }

    Vector2? FindNextWall(Vector2 direction)
    {
        Vector2 rayOrigin = rb.position + Vector2.up * context.playerMovementConfig.WallHopMaxDistance.y + direction * 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, context.playerMovementConfig.WallHopMaxDistance.x, context.wallLayer);
        Debug.DrawRay(rayOrigin, direction * context.playerMovementConfig.WallHopMaxDistance.x, Color.red);
        if (hit.collider != null)
        {
            return hit.point + (Vector2)hit.normal * 0.4f; // Offset to stick to the wall slightly
        }
        return null;
    }

    Vector2? FindFirstWall(Vector2 direction)
    {
        Vector2 wallDirection = -direction;
        Vector2 rayOrigin = rb.position + Vector2.up * context.playerMovementConfig.WallHopMaxDistance.y - wallDirection * 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, wallDirection, context.playerMovementConfig.WallHopMaxDistance.x, context.wallLayer);
        Debug.DrawRay(rayOrigin, wallDirection * context.playerMovementConfig.WallHopMaxDistance.x, Color.blue);
        if (hit.collider != null)
        {
            return hit.point + (Vector2)hit.normal * 0.4f; // Offset to stick to the wall slightly
        }
        return null;
    }
}