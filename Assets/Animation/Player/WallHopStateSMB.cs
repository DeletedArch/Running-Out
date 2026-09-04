using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class WallHopStateSMB : StateMachineBehaviour
{
    [SerializeField] private float wallHopDuration = 0.15f;

    private CancellationTokenSource stateCts;
    private Rigidbody2D rb;
    private PlayerContext context;
    private float originalGravityScale = 1f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        stateCts?.Cancel();
        stateCts?.Dispose();
        stateCts = new CancellationTokenSource();

        PlayerContext context = animator.GetComponent<PlayerController>().Context;
        if (context != null)
        {
            Vector2 wallHopDirection = animator.transform.localScale.x > 0 ? Vector2.left : Vector2.right;
            this.context = context;
            rb = context.playerRigidbody;
            originalGravityScale = rb.gravityScale;
            rb.gravityScale = 0f;

            Vector2? nextWallPosition = FindNextWall(wallHopDirection);
            Vector2 finalHopPosition;
            if (nextWallPosition.HasValue)
            {
                finalHopPosition = nextWallPosition.Value + Vector2.up * context.playerMovementConfig.WallHopMaxDistance.y;
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
        rb.transform.localScale = new Vector3(-rb.transform.localScale.x, rb.transform.localScale.y, rb.transform.localScale.z);
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
        RaycastHit2D hit = Physics2D.Raycast(rb.position + Vector2.up * context.playerMovementConfig.WallHopMaxDistance.y, direction, context.playerMovementConfig.WallHopMaxDistance.x, context.wallLayer);
        Debug.DrawRay(rb.position + Vector2.up * context.playerMovementConfig.WallHopMaxDistance.y, direction * context.playerMovementConfig.WallHopMaxDistance.x, Color.red);
        if (hit.collider != null)
        {
            return hit.point + Vector2.right * (direction.x > 0 ? 0.4f : -0.4f); // Offset to stick to the wall slightly
        }
        return null;
    }
}