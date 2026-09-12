using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public class JumpStateSMB : StateMachineBehaviour
{
    private Rigidbody2D rb;
    private PlayerMovementConfig movementConfig;
    private float originalGravityScale = 1f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var player = animator.GetComponent<PlayerController>();
        if (player == null) return;

        rb = player.Context.playerRigidbody;
        movementConfig = player.Context.playerMovementConfig;
        originalGravityScale = rb.gravityScale;
        if (animator.GetBool("IsGrounded") && animator.GetBool("Jump"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, movementConfig.JumpForce);
            VFXEvents.TriggerVFX("JumpOff", rb.position, Quaternion.identity, false);
        }
        UniTask.WaitUntil(() =>
        {
            return !animator.GetBool("IsGrounded");
        }).ContinueWith(() =>
        {
            animator.SetBool("Jump", false);
        }).Forget();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (rb != null)
        {
            rb.gravityScale = originalGravityScale;
        }
        if (animator.GetBool("IsGrounded"))
        {
            VFXEvents.TriggerVFX("Land", rb.position, Quaternion.identity, false);
        }
    }
}