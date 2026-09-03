using UnityEngine;
using Cysharp.Threading.Tasks;

public class WallHopStateSMB : StateMachineBehaviour
{
    [SerializeField] private float wallHopDuration = 0.15f;

    private Rigidbody2D rb;
    private PlayerContext context;
    private float originalGravityScale = 1f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
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
            ApplyWallHop(rb, finalHopPosition);
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (rb == null || context == null) return;

        rb.linearVelocity = Vector2.zero;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (rb != null)
        {
            rb.gravityScale = originalGravityScale;
        }
        animator.SetBool("WallHopping", false);
    }

    UniTask ApplyWallHop(Rigidbody2D rb, Vector2 wallHopPosition)
    {
        rb.transform.localScale = new Vector3(-rb.transform.localScale.x, rb.transform.localScale.y, rb.transform.localScale.z);
        Vector2 startPosition = rb.position;
        float elapsedTime = 0f;
        return UniTask.WaitUntil(() =>
        {
            elapsedTime += Time.deltaTime;
            rb.MovePosition(Vector2.Lerp(startPosition, wallHopPosition, elapsedTime / wallHopDuration));
            return elapsedTime >= wallHopDuration;
        }).ContinueWith(() =>
        {
            rb.gravityScale = originalGravityScale;
            context.playerAnimator.SetBool("WallHopping", false);
        });
    }

    Vector2? FindNextWall(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(rb.position * Vector2.up * context.playerMovementConfig.WallHopMaxDistance.y, direction, context.playerMovementConfig.WallHopMaxDistance.x, context.wallLayer);
        Debug.DrawRay(rb.position * Vector2.up * context.playerMovementConfig.WallHopMaxDistance.y, direction * context.playerMovementConfig.WallHopMaxDistance.x, Color.red);
        if (hit.collider != null)
        {
            return hit.point;
        }
        return null;
    }
}