using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerContext context;

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
        InputController.OnMoveInput += HandleMoveInput;
        InputController.OnJumpStart += HandleJumpInput;
        InputController.OnDashInput += HandleDashInput;
        InputController.OnAttackStart += HandleAttackInput;
        InputController.OnBlockStart += HandleBlockInput;
        InputController.OnSwiftDashInput += HandleSwiftDashInput;
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
        if (!CheckAnimatorState("Dash"))
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
        if (moveInput.x == 0 || moveInput == Vector2.zero) return;
        Vector2 velocity = context.playerRigidbody.linearVelocity;
        Vector2 newVelocity = new Vector2(velocity.x + moveInput.x * context.playerMovementConfig.RunSpeed, velocity.y);
        context.playerRigidbody.linearVelocity = newVelocity;
    }

    UniTask Dash()
    {
        float distancePassed = 0f;
        context.playerAnimator.SetBool("Dash", true);
        return UniTask.WaitUntil(() =>
        {
            context.playerRigidbody.linearVelocity = new Vector2(Mathf.Sign(transform.localScale.x) * context.playerMovementConfig.DashForce, 0);
            distancePassed += context.playerRigidbody.linearVelocity.magnitude * Time.fixedDeltaTime;
            return distancePassed >= context.playerMovementConfig.DashDistance;
        }).ContinueWith(() =>
        {
            context.playerAnimator.SetBool("Dash", false);
        });
    }

    void HandleMoveInput(Vector2 moveInput)
    {
        context.moveInput = moveInput;
    }

    void HandleJumpInput()
    {
        if (!IsGrounded()) { Debug.Log("Player is not grounded. Cannot jump."); return; }
        context.playerRigidbody.linearVelocity = new Vector2(context.playerRigidbody.linearVelocity.x, context.playerMovementConfig.JumpForce);
        context.playerAnimator.SetBool("Jump", true);
        UniTask.Delay(TimeSpan.FromSeconds(0.25f)).ContinueWith(() =>
        {
            context.playerAnimator.SetBool("Jump", false);
        }).Forget();
    }

    void HandleDashInput()
    {
        if (CheckAnimatorState("Dash")) return;
        Dash().Forget();
    }

    void HandleSwiftDashInput()
    {

    }

    void HandleAttackInput()
    {

    }

    void HandleBlockInput()
    {

    }

    bool CheckAnimatorState(string stateName)
    {
        return context.playerAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }
}