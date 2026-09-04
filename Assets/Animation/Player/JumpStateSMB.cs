using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public class JumpStateSMB : StateMachineBehaviour
{
    private Vector2 startPosition;
    private Rigidbody2D rb;
    private PlayerMovementConfig movementConfig;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var player = animator.GetComponent<PlayerController>();
        if (player == null) return;

        rb = player.Context.playerRigidbody;
        movementConfig = player.Context.playerMovementConfig;
        startPosition = rb.position;
        if (animator.GetBool("IsGrounded"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, movementConfig.JumpForce * animator.GetFloat("Timer"));
        }
        UniTask.WaitUntil(() =>
        {
            return !animator.GetBool("IsGrounded");
        }).ContinueWith(() =>
        {
            player.Context.playerAnimator.SetBool("Jump", false);
        }).Forget();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       
    }
}