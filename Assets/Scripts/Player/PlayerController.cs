using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerContext context;
    [SerializeField] private PlayerCombat playerCombat;
    private RangeDetectionHelper rangeDetectionHelper;
    public PlayerContext Context => context;
    void Validate()
    {
        if (context == null)
        {
            Debug.LogError("PlayerContext is not assigned in the PlayerController.");
        }
    }

    void Awake()
    {
        Validate();
        rangeDetectionHelper = GetComponent<RangeDetectionHelper>();
        context.overrideController = new AnimatorOverrideController(context.playerAnimator.runtimeAnimatorController);
        context.playerAnimator.runtimeAnimatorController = context.overrideController;
        playerCombat = new PlayerCombat(context, rangeDetectionHelper)
        {
            GetPlayerDirection = GetPlayerDirection
        };
        InputController.OnMoveInput += HandleMoveInput;
        InputController.OnJumpStart += HandleJumpInput;
        InputController.OnDashInput += HandleDashInput;
        InputController.OnAttackStart += HandleAttackInput;
        InputController.OnAttackEnd += HandleAttackRelease;
        InputController.OnBlockStart += HandleBlockInput;
        InputController.OnSwiftDashInput += HandleSwiftDashInput;
        SetStateSMB.OnStateEntered += (str) => { Debug.Log("State Entered: " + str); context.currentState = str; };
        SetStateSMB.OnStateExited += (str) => { Debug.Log("State Exited: " + str); };
    }

    void FixedUpdate()
    {
        MovePlayer(context.moveInput);
        ApplyCustomDrag();
    }

    void Update()
    {
        LimitSpeed();
        AdjustOrientation(context.moveInput.x);
        IsGrounded();
    }

    public bool IsGrounded()
    {
        Debug.DrawRay(transform.position, Vector2.down * 1.05f, Color.red);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.05f, context.groundLayer);
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

    void LimitSpeed()
    {
        if (context.currentState != "Dash")
        {
            Vector2 velocity = context.playerRigidbody.linearVelocity;
            if (Mathf.Abs(velocity.x) > context.playerMovementConfig.MaxSpeed)
            {
                Vector2 newVelocity = new Vector2(Mathf.Sign(velocity.x) * context.playerMovementConfig.MaxSpeed, velocity.y);
                context.playerRigidbody.linearVelocity = newVelocity;
            }
        }
        context.playerAnimator.SetFloat("velocityX", Mathf.Abs(context.playerRigidbody.linearVelocity.x));
        context.playerAnimator.SetFloat("velocityY", context.playerRigidbody.linearVelocity.y);
    }

    void AdjustOrientation(float moveInputX)
    {
        if (moveInputX == 0 || !context.canMove) return;
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
        if (moveInput.x == 0 || moveInput == Vector2.zero || !context.canMove && !context.canCancel) return;
        if (context.canCancel)
        {
            context.playerAnimator.Play("Run", 0, 0f);
        }
        Vector2 velocity = context.playerRigidbody.linearVelocity;
        Vector2 newVelocity = new Vector2(velocity.x + moveInput.x * context.playerMovementConfig.RunSpeed, velocity.y);
        context.playerRigidbody.linearVelocity = newVelocity;
    }

    void HandleMoveInput(Vector2 moveInput)
    {
        context.moveInput = moveInput;
    }

    void HandleJumpInput()
    {
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

    public void SetMovement(bool canMove)
    {
        context.canMove = canMove;
        if (canMove) {
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
            return new Vector2(Mathf.Sign(context.moveInput.x), Mathf.Sign(context.moveInput.y));
        }
        else
        {
            return new Vector2(Mathf.Sign(transform.localScale.x), 0); 
        }
    }
}