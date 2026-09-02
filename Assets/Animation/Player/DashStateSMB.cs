using UnityEngine;
using System;

public class DashStateSMB : StateMachineBehaviour
{
    private Vector2 startPosition;
    private Rigidbody2D rb;
    private PlayerMovementConfig movementConfig;
    private float dashDirection;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var player = animator.GetComponent<PlayerController>();
        if (player == null) return;

        rb = player.Context.playerRigidbody;
        movementConfig = player.Context.playerMovementConfig;
        startPosition = rb.position;
        dashDirection = Mathf.Sign(player.transform.localScale.x);

        rb.linearVelocity = new Vector2(dashDirection * movementConfig.DashForce, 0);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (rb == null || movementConfig == null) return;

        rb.linearVelocity = new Vector2(dashDirection * movementConfig.DashForce, 0);
        float distanceTraveled = Vector2.Distance(startPosition, rb.position);
        if (distanceTraveled >= movementConfig.DashDistance)
        {
            animator.SetBool("Dash", false);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }
}