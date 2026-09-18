using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.UI;
using Unity.Cinemachine;

public enum WallTouchDirection
{
    Left = -1,
    None = 0,
    Right = 1
}

public class PlayerController : MonoBehaviour, IEntity
{
    [SerializeField] private PlayerContext context;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private RangeDetectionHelper[] rangeDetectionHelper;
    [SerializeField] private TimerSystem timerSystem;
    [SerializeField] private CooldownSystem cooldownSystem;
    public CinemachineImpulseSource impulseSource;
    public PlayerContext Context => context;
    public float Health => new float(); // TODO: Add timer and include it as health property

    [Header("Ledge Grab Settings")]
    [SerializeField] private float ledgeWallCheckDistance = 0.75f;
    [SerializeField] private float ledgeWallCheckOffsetY = 0.2f;
    [SerializeField] private float ledgeCheckHeight = 1.1f;
    [SerializeField] private float ledgeDownwardDistance = 1.2f;
    [SerializeField] private float ceilingCheckDistance = 1.6f;
    [SerializeField] private Vector2 ledgePushForce = new Vector2(6f, 20f);
    [SerializeField] private string ledgeGrabCooldownKey = "LedgeGrab";
    void Validate()
    {
        if (context == null)
        {
            Debug.LogError("PlayerContext is not assigned in the PlayerController.");
        }
        if (context.playerRigidbody == null)
        {
            Debug.LogError("Player Rigidbody2D is not assigned in the PlayerContext.");
        }
        if (context.playerAnimator == null)
        {
            Debug.LogError("Player Animator is not assigned in the PlayerContext.");
        }
        if (context.playerSpriteRenderer == null)
        {
            Debug.LogError("Player Sprite Renderer is not assigned in the PlayerContext.");
        }
    }

    void Awake()
    {
        Validate();
        context.overrideController = new AnimatorOverrideController(context.playerAnimator.runtimeAnimatorController);
        context.playerAnimator.runtimeAnimatorController = context.overrideController;
        playerCombat = new PlayerCombat(context, rangeDetectionHelper)
        {
            GetPlayerDirection = GetPlayerDirection
        };
    }

    void Start()
    {
        cooldownSystem?.Initialize();
    }

    void OnEnable()
    {
        InputController.OnMoveInput += HandleMoveInput;
        InputController.OnJumpStart += HandleJumpInput;
        InputController.OnDashInput += HandleDashInput;
        InputController.OnAttackStart += HandleAttackInput;
        InputController.OnAttackEnd += HandleAttackRelease;
        InputController.OnBlockStart += HandleBlockInput;
        InputController.OnBlockEnd += HandleBlockRelease;
        InputController.OnSwiftDashInput += HandleSwiftDashInput;
        SetStateSMB.OnStateEntered += HandleStateEntered;
        SetStateSMB.OnStateExited += HandleStateExited;
        playerCombat.OnParried += HandleOnParried;
        playerCombat.OnBlocked += HandleOnBlocked;
        TimerSystem.OnTimerDepleted += Die;

        // Debug
        InputController.OnDebugInput1 += OnInputDebug1;
    }

    void OnDisable()
    {
        InputController.OnMoveInput -= HandleMoveInput;
        InputController.OnJumpStart -= HandleJumpInput;
        InputController.OnDashInput -= HandleDashInput;
        InputController.OnAttackStart -= HandleAttackInput;
        InputController.OnAttackEnd -= HandleAttackRelease;
        InputController.OnBlockStart -= HandleBlockInput;
        InputController.OnBlockEnd -= HandleBlockRelease;
        InputController.OnSwiftDashInput -= HandleSwiftDashInput;
        SetStateSMB.OnStateEntered -= HandleStateEntered;
        SetStateSMB.OnStateExited -= HandleStateExited;
        playerCombat.OnParried -= HandleOnParried;
        playerCombat.OnBlocked -= HandleOnBlocked;
        TimerSystem.OnTimerDepleted -= Die;

        // Debug
        InputController.OnDebugInput1 -= OnInputDebug1;
    }

    private void HandleStateEntered(string stateTag)
    {
        Debug.Log("State Entered: " + stateTag);
        context.currentState = stateTag;
    }

    private void HandleStateExited(string stateTag)
    {
        Debug.Log("State Exited: " + stateTag);
    }

    void FixedUpdate()
    {
        MovePlayer(context.moveInput);
        LimitSpeed();
        ApplyCustomDrag();
    }

    void Update()
    {
        AdjustOrientation(context.moveInput.x);
        IsGrounded();
        if (context.moveInput != Vector2.zero)
        {
            LedgeGrab();
        }
        playerCombat?.Update();
        context.playerAnimator.SetInteger("TouchingWall", (int)GetWallTouchDirection());
        timerSystem?.Update(Time.deltaTime);
        cooldownSystem?.UpdateCooldowns();
        // SetTimer(); -- Debug Only
    }

    bool flip = false;
    void SetTimer()
    {
        float currentTimer = context.playerAnimator.GetFloat("Timer");
        if (flip)
        {
            context.playerAnimator.SetFloat("Timer", currentTimer + Time.deltaTime);
            if (currentTimer >= 1.5f)
            {
                flip = false;
            }
        }
        else
        {
            context.playerAnimator.SetFloat("Timer", currentTimer - Time.deltaTime);
            if (currentTimer <= 0.5f)
            {
                flip = true;
            }
        }
    }

    public bool IsGrounded()
    {
        // Debug.DrawRay(transform.position, Vector2.down * 1.05f, Color.red);
        RaycastHit2D hit = Physics2D.BoxCast(transform.position,
        new Vector2(Mathf.Abs(transform.localScale.x) - 0.4f, transform.localScale.y * 0.5f), 0f, Vector2.down, transform.localScale.y, context.groundLayer);
        if (hit.collider != null)
        {
            context.playerAnimator.SetBool("IsGrounded", true);
        }
        else
        {
            context.playerAnimator.SetBool("IsGrounded", false);
        }
        return hit.collider != null;
    }

    public bool CheckWallAndGround(Vector2 origin, Vector2 direction, float distance, out RaycastHit2D hit)
    {
        LayerMask wallAndGround = context.wallLayer | context.groundLayer;
        hit = Physics2D.Raycast(origin, direction, distance, wallAndGround);
        return hit.collider != null;
    }

    public bool IsTouchingWall()
    {
        float facingDir = Mathf.Sign(transform.localScale.x);
        Debug.DrawRay(transform.position, facingDir * transform.right * ledgeWallCheckDistance, Color.blue);
        return CheckWallAndGround(transform.position, Vector2.right * facingDir, ledgeWallCheckDistance, out _);
    }

    public bool CheckWallAndGround(out Vector2 targetLedgePosition)
    {
        targetLedgePosition = Vector2.zero;

        float facingDir = Mathf.Sign(transform.localScale.x);
        Vector2 facingVector = new Vector2(facingDir, 0f);
        LayerMask wallAndGround = context.wallLayer | context.groundLayer;

        Vector2 waistOrigin = (Vector2)transform.position + Vector2.up * ledgeWallCheckOffsetY;
        RaycastHit2D wallHit;
        if (!CheckWallAndGround(waistOrigin, facingVector, ledgeWallCheckDistance, out wallHit))
        {
            return false;
        }

        Vector2 aboveOrigin = (Vector2)transform.position + Vector2.up * ledgeCheckHeight;
        RaycastHit2D highHit;
        if (CheckWallAndGround(aboveOrigin, facingVector, ledgeWallCheckDistance + 0.15f, out highHit))
        {
            return false;
        }

        Vector2 downOrigin = new Vector2(wallHit.point.x + facingDir * 0.2f, transform.position.y + ledgeCheckHeight);
        RaycastHit2D groundHit = Physics2D.Raycast(downOrigin, Vector2.down, ledgeDownwardDistance, wallAndGround);
        Debug.DrawRay(downOrigin, Vector2.down * ledgeDownwardDistance, Color.green);

        if (groundHit.collider == null)
        {
            return false;
        }

        if (groundHit.point.y < transform.position.y - 0.2f)
        {
            return false;
        }

        targetLedgePosition = groundHit.point;
        return true;
    }

    public bool CheckCeiling(Vector2 origin, float distance)
    {
        LayerMask wallAndGround = context.wallLayer | context.groundLayer;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up, distance, wallAndGround);
        Debug.DrawRay(origin, Vector2.up * distance, Color.magenta);
        return hit.collider != null;
    }

    public bool LedgeGrab()
    {
        if (cooldownSystem != null && cooldownSystem.IsOnCooldown(ledgeGrabCooldownKey)) return false;
        if (IsGrounded()) return false;
        if (context.currentState == "Death" || context.currentState == "Die" || context.currentState == "SwiftDash" || context.currentState == "WallHop") return false;

        float facingDir = Mathf.Sign(transform.localScale.x);

        if (context.moveInput.x != 0 && Mathf.Sign(context.moveInput.x) != facingDir)
            return false;

        if (!CheckWallAndGround(out Vector2 targetLedgePosition))
            return false;

        if (CheckCeiling(transform.position, ceilingCheckDistance))
        {
            return false;
        }

        Vector2 ledgeLandingTarget = new Vector2(targetLedgePosition.x + facingDir * 0.3f, targetLedgePosition.y + 0.1f);
        if (CheckCeiling(ledgeLandingTarget, ceilingCheckDistance))
        {
            return false;
        }

        // Push the player towards up the ledge
        context.playerRigidbody.linearVelocity = new Vector2(facingDir * ledgePushForce.x, ledgePushForce.y);
        cooldownSystem?.UseCooldown(ledgeGrabCooldownKey);

        (context.playerMovementConfig?.WallJumpSound ?? context.playerMovementConfig?.JumpSound)?.Play(transform.position);

        return true;
    }

    public WallTouchDirection GetWallTouchDirection()
    {
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, transform.right, 0.6f, context.wallLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, -transform.right, 0.6f, context.wallLayer);
        Debug.DrawRay(transform.position, transform.right * 0.6f, Color.blue);
        Debug.DrawRay(transform.position, -transform.right * 0.6f, Color.blue);
        if (hitRight.collider != null && hitLeft.collider != null)
        {
            float DistanceToRightWall = hitRight.distance;
            float DistanceToLeftWall = hitLeft.distance;
            if (DistanceToLeftWall < DistanceToRightWall)
            {
                return WallTouchDirection.Left;
            }
            else
            {
                return WallTouchDirection.Right;
            }
        }
        else if (hitLeft.collider != null && hitRight.collider == null)
        {
            return WallTouchDirection.Left;
        }
        else if (hitRight.collider != null && hitLeft.collider == null)
        {
            return WallTouchDirection.Right;
        }
        return WallTouchDirection.None;
    }

    void LimitSpeed()
    {
        if (context.currentState != "Dash")
        {
            Vector2 velocity = context.playerRigidbody.linearVelocity;
            if (Mathf.Abs(velocity.x) > context.playerMovementConfig.MaxSpeed * timerSystem.NormalizedTimer)
            {
                Vector2 newVelocity = new Vector2(Mathf.Sign(velocity.x) * context.playerMovementConfig.MaxSpeed * timerSystem.NormalizedTimer, velocity.y);
                context.playerRigidbody.linearVelocity = newVelocity;
            }
        }
        context.playerAnimator.SetFloat("velocityX", Mathf.Abs(context.playerRigidbody.linearVelocity.x));
        context.playerAnimator.SetFloat("velocityY", context.playerRigidbody.linearVelocity.y);
    }

    void AdjustOrientation(float moveInputX)
    {
        if (moveInputX == 0 || !context.canMove || context.currentState == "WallHop") return;
        transform.localScale = new Vector3(Mathf.Sign(moveInputX) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void ApplyCustomDrag()
    {
        if (!context.useDrag) return;
        if (Math.Abs(context.playerRigidbody.linearVelocity.x) < 0.1f)
        {
            context.playerRigidbody.linearVelocity = new Vector2(0, context.playerRigidbody.linearVelocity.y);
            return;
        }
        float dragValue = context.customDrag;
        context.playerRigidbody.linearVelocity = new Vector2(context.playerRigidbody.linearVelocity.x * (1 - dragValue), context.playerRigidbody.linearVelocity.y);

    }

    void MovePlayer(Vector2 moveInput)
    {
        if (moveInput.x == 0 || moveInput == Vector2.zero || !context.canMove) return;
        Vector2 velocity = context.playerRigidbody.linearVelocity;
        Vector2 newVelocity = new Vector2(velocity.x + moveInput.x * context.playerMovementConfig.RunSpeed * timerSystem.NormalizedTimer, velocity.y);
        context.playerRigidbody.linearVelocity = newVelocity;
    }

    void HandleMoveInput(Vector2 moveInput)
    {
        context.moveInput = moveInput;
    }

    void HandleJumpInput()
    {
        if (GetWallTouchDirection() != WallTouchDirection.None)
        {
            context.playerAnimator.SetBool("WallHopping", true);
            context.playerAnimator.SetTrigger("WallHop");
            if (context.canCancel && context.currentState == "WallHop")
            {
                context.playerAnimator.Play("WallHop", 0, 0f);
            }
            return;
        }
        else
        {
            Debug.Log("Player is not touching a wall. Cannot perform wall hop.");
            Debug.Log(context.currentState);
        }
        if (!IsGrounded() || context.currentState == "Jump") { Debug.Log("Player is not grounded. Cannot jump."); return; }
        context.playerAnimator.SetBool("Jump", true);
        if (context.canCancel)
        {
            context.playerAnimator.Play("Jump", 0, 0f);
        }
    }

    void HandleDashInput()
    {
        if (context.currentState == "Dash" || cooldownSystem.IsOnCooldown("Dash")) return;
        context.playerAnimator.SetBool("Dash", true);
        if (context.canCancel)
        {
            context.playerAnimator.Play("Dash", 0, 0f);
        }
    }

    void HandleSwiftDashInput()
    {
        if (context.currentState == "SwiftDash" || cooldownSystem.IsOnCooldown("SwiftDash")) return;
        if (context.swiftDashChannel.GetBestTarget() == null) return;
        context.playerAnimator.SetBool("SDash", true);
        if (context.canCancel)
        {
            context.playerAnimator.Play("SwiftDash", 0, 0f);
        }
    }

    void HandleAttackInput()
    {
        playerCombat.HandleAttackInput();
    }

    private void HandleAttackRelease()
    {
        playerCombat.HandleAttackRelease();
    }

    void HandleBlockInput()
    {
        playerCombat.HandleBlockInput();
    }
    void HandleBlockRelease()
    {
        playerCombat.HandleBlockRelease();
    }

    void SpriteColorFlash(Color color, float duration)
    {
        if (context.playerSpriteRenderer == null) return;
        context.playerSpriteRenderer.color = color;
        UniTask.Delay(TimeSpan.FromSeconds(duration)).ContinueWith(() =>
        {
            context.playerSpriteRenderer.color = Color.white;
        }).Forget();
    }

    void SpriteWhiteFlash(float duration)
    {
        if (context.playerSpriteRenderer == null) return;
        context.playerSpriteRenderer.material = context.spriteFlashMaterial;
        context.playerSpriteRenderer.color = Color.white;
        UniTask.Delay(TimeSpan.FromSeconds(duration)).ContinueWith(() =>
        {
            context.playerSpriteRenderer.material = context.originalMaterial;
            context.playerSpriteRenderer.color = Color.white;
        }).Forget();
    }

    public void UseCooldown(string key)
    {
        if (cooldownSystem.UseCooldown(key))
        {
            Debug.Log($"Cooldown used for key: {key}");
        }
        else
        {
            Debug.Log($"Cooldown not available for key: {key}");
        }
    }

    public void SetMovement(bool canMove)
    {
        context.canMove = canMove;
        if (canMove)
        {
            context.playerRigidbody.WakeUp();
        }
    }

    public void SetInvincible(bool isInvincible)
    {
        context.isInvincible = isInvincible;
    }

    public void SetCanCancel(bool canCancel)
    {
        context.canCancel = canCancel;
    }

    public Vector2 GetPlayerDirection()
    {
        if (context.moveInput != Vector2.zero)
        {
            return new Vector2(context.moveInput.x, context.moveInput.y);
        }
        else
        {
            return new Vector2(Mathf.Sign(transform.localScale.x), 0);
        }
    }

    public void TakeDamage(float amount)
    {
        if (context.currentState == "Death") return;
        playerCombat.HandleGettingHit(amount);
        if (context.currentState == "Dash")
        {
            timerSystem?.ReplenishTimer(3f);
            return;
        }
        if (context.isInvincible || context.currentState == "Block" || cooldownSystem.IsOnCooldown("Damage")) return;
        cooldownSystem.UseCooldown("Damage");
        timerSystem?.DepleteTimer(amount, TimerAction.DepleteHit);
        context.playerAnimator.SetTrigger("Hit");
        // SpriteColorFlash(Color.red, 0.15f);
        SpriteWhiteFlash(0.15f);
        impulseSource?.GenerateImpulse();
        ActionHelpers.ApplyGlobalHitstop(0.05f).Forget();
        context.playerCombatConfig.StaggerSound?.Play(transform.position);

        // Implement damage logic here
        Debug.Log($"Player took {amount} damage.");
    }

    public void TakeDamage(float amount, GameObject source)
    {
        if (context.currentState == "Death") return;
        playerCombat.HandleGettingHit(amount, source);
        if (context.currentState == "Dash")
        {
            timerSystem?.ReplenishTimer(3f);
            return;
        }
        if (context.isInvincible || context.currentState == "Block" || cooldownSystem.IsOnCooldown("Damage")) return;
        cooldownSystem.UseCooldown("Damage");
        timerSystem?.DepleteTimer(amount, TimerAction.DepleteHit);
        context.playerAnimator.SetTrigger("Hit");
        // SpriteColorFlash(Color.red, 0.15f);
        SpriteWhiteFlash(0.15f);
        impulseSource?.GenerateImpulse();
        ActionHelpers.ApplyGlobalHitstop(0.05f).Forget();
        context.playerCombatConfig.StaggerSound?.Play(transform.position);

        // Implement damage logic here
        Debug.Log($"Player took {amount} damage from {source.name}.");
    }

    public void HandleOnParried(GameObject source)
    {
        timerSystem?.ReplenishTimer(3f);
        context.playerRigidbody.AddForce(-Vector2.right * GetPlayerDirection().normalized.x * 15f, ForceMode2D.Impulse);
        if (source != null)
        {
            var enemyRb = source.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                enemyRb.AddForce(Vector2.right * GetPlayerDirection().normalized.x * 15f, ForceMode2D.Impulse);
            }
        }
    }

    public void HandleOnBlocked(float amount, GameObject source)
    {
        timerSystem?.DepleteTimer(amount / 2f, TimerAction.DepleteBlock);
        context.playerRigidbody.AddForce(-Vector2.right * GetPlayerDirection().normalized.x * 5f, ForceMode2D.Impulse);
        SpriteColorFlash(new Color(0f, 0.9f, 1f), 0.15f);
        if (source != null)
        {
            var enemyRb = source.GetComponent<Rigidbody2D>();
            if (enemyRb != null)
            {
                enemyRb.AddForce(Vector2.right * GetPlayerDirection().normalized.x * 5f, ForceMode2D.Impulse);
            }
        }
    }

    public void Die()
    {
        // Implement death logic here
        Debug.Log("Player died.");
        context.playerAnimator.SetTrigger("Die");
        ActionHelpers.ApplyGlobalHitstop(0.15f).Forget();
        impulseSource?.GenerateImpulseWithForce(1f);
    }

    // void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (other.CompareTag("Breakable"))
    //     {
    //         Breakable breakable = other.GetComponent<Breakable>();
    //         if (breakable != null)
    //         {
    //             impulseSource?.GenerateImpulseWithForce(0.5f);
    //         }
    //     }
    // }

    // DEBUGGGGGGGG EVERYTHIGNG IS DEBUGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position - Vector3.up * transform.localScale.y, transform.localScale - Vector3.up * 0.5f * transform.localScale.y - Vector3.right * 0.4f);
    }

    void OnDrawGizmosSelected()
    {
        float facingDir = Mathf.Sign(transform.localScale.x);
        Vector2 facingVector = new Vector2(facingDir, 0f);

        // Wall check ray (cyan)
        Gizmos.color = Color.cyan;
        Vector2 waistOrigin = (Vector2)transform.position + Vector2.up * ledgeWallCheckOffsetY;
        Gizmos.DrawLine(waistOrigin, waistOrigin + facingVector * ledgeWallCheckDistance);

        // Clearance ray above ledge (blue)
        Gizmos.color = Color.blue;
        Vector2 aboveOrigin = (Vector2)transform.position + Vector2.up * ledgeCheckHeight;
        Gizmos.DrawLine(aboveOrigin, aboveOrigin + facingVector * (ledgeWallCheckDistance + 0.15f));

        // Ground top surface downward ray (green)
        Gizmos.color = Color.green;
        Vector2 downOrigin = aboveOrigin + facingVector * (ledgeWallCheckDistance + 0.15f);
        Gizmos.DrawLine(downOrigin, downOrigin + Vector2.down * ledgeDownwardDistance);

        // Ceiling check ray (magenta)
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + Vector2.up * ceilingCheckDistance);
    }

    // Take damage debug
    void OnInputDebug1()
    {
        TakeDamage(0.5f);
    }
}