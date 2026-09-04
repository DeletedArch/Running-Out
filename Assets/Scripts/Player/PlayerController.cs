using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IEntity
{
    [SerializeField] private PlayerContext context;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private RangeDetectionHelper[] rangeDetectionHelper;
    [SerializeField] private TimerSystem timerSystem;
    public PlayerContext Context => context;
    public float Health => new float(); // TODO: Add timer and include it as health property
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
        timerSystem = new TimerSystem(context.playerAnimator, 20f);
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
        playerCombat?.Update();
        IsTouchingWall();
        timerSystem?.Update(Time.deltaTime);
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
        new Vector2(Mathf.Abs(transform.localScale.x) - 0.1f, transform.localScale.y * 0.5f), 0f, Vector2.down, transform.localScale.y, context.groundLayer);
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

    public bool IsTouchingWall()
    {
        Debug.DrawRay(transform.position, Mathf.Sign(transform.localScale.x) * transform.right * 0.75f, Color.blue);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Mathf.Sign(transform.localScale.x) * transform.right, 0.6f, context.wallLayer);
        return hit.collider != null;
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
        if (!context.useDrag || !IsGrounded()) return;
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
        if (IsTouchingWall())
        {
            context.playerAnimator.SetBool("WallHopping", true);
            context.playerAnimator.SetTrigger("WallHop");
            if (context.canCancel && context.currentState == "WallHop")
            {
                context.playerAnimator.Play("WallHop", 0, 0f);
            }
            return;
        } else
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
        if (context.currentState == "Dash") return;
        context.playerAnimator.SetBool("Dash", true);
        if (context.canCancel)
        {
            context.playerAnimator.Play("Dash", 0, 0f);
        }
    }

    void HandleSwiftDashInput()
    {
        if (context.currentState == "SwiftDash") return;
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
        if (context.isInvincible) return;
        // Implement damage logic here
        Debug.Log($"Player took {amount} damage.");
    }

    public void Die()
    {
        // Implement death logic here
        Debug.Log("Player died.");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position - Vector3.up * transform.localScale.y, transform.localScale - Vector3.up * 0.5f * transform.localScale.y - Vector3.right * 0.1f);
    }
}