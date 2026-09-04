using UnityEngine;

public class DashStateSMB : StateMachineBehaviour, ITimerAccess
{
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private float timerUsage = 1f;
    [SerializeField] private float timerRestoration = 2f;

    public float TimerUsage => timerUsage;
    public float TimerRestoration => timerRestoration;

    private Vector2 startPosition;
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private PlayerMovementConfig movementConfig;
    private float dashDirection;
    private float elapsedTime;
    private float maxDashDuration;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var player = animator.GetComponent<PlayerController>();
        if (player == null) return;

        rb = player.Context.playerRigidbody;
        playerCollider = player.GetComponent<Collider2D>();
        movementConfig = player.Context.playerMovementConfig;

        startPosition = rb.position;
        Vector2 direction = player.GetPlayerDirection();
        dashDirection = direction.x != 0 ? Mathf.Sign(direction.x) : Mathf.Sign(player.transform.localScale.x);
        player.transform.localScale = new Vector3(Mathf.Sign(dashDirection) * Mathf.Abs(player.transform.localScale.x), player.transform.localScale.y, player.transform.localScale.z);
        elapsedTime = 0f;

        maxDashDuration = (movementConfig.DashDistance / movementConfig.DashForce) + 0.1f;

        ITimerAccess.ModifyTimer(-timerUsage); // Deduct timer usage when the dash starts
        rb.linearVelocity = new Vector2(dashDirection * movementConfig.DashForce * animator.GetFloat("Timer"), 0);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (rb == null || movementConfig == null) return;

        elapsedTime += Time.fixedDeltaTime;

        Vector2 checkOrigin = playerCollider != null ? (Vector2)playerCollider.bounds.center : rb.position;
        Vector2 checkDirection = new Vector2(dashDirection, 0f);

        RaycastHit2D wallHit = Physics2D.Raycast(checkOrigin, checkDirection, wallCheckDistance, wallMask);

        Debug.DrawRay(checkOrigin, checkDirection * wallCheckDistance, Color.darkOrange);

        if (wallHit.collider != null)
        {
            animator.SetBool("Dash", false);
            return;
        }

        float distanceTraveled = Vector2.Distance(startPosition, rb.position);
        if (distanceTraveled >= movementConfig.DashDistance || elapsedTime >= maxDashDuration / animator.GetFloat("Timer"))
        {
            animator.SetBool("Dash", false);
            return;
        }

        rb.linearVelocity = new Vector2(dashDirection * movementConfig.DashForce * animator.GetFloat("Timer"), 0);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }
}