using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class PlayerCombat
{
    private PlayerCombatConfig config;
    private PlayerContext context;

    private int currentComboCount = 0;
    private float lastAttackTime = 0f;
    private float attackHoldTime = 0f;
    private RangeDetectionHelper[] rangeDetectionHelper;

    public delegate Vector2 GetPlayerDirectionDelegate();
    public GetPlayerDirectionDelegate GetPlayerDirection;

    public PlayerCombat(PlayerContext context, RangeDetectionHelper[] rangeDetectionHelper)
    {
        this.config = context.playerCombatConfig;
        this.context = context;
        this.rangeDetectionHelper = rangeDetectionHelper;

        if (rangeDetectionHelper != null)
        {
            foreach (var helper in rangeDetectionHelper)
            {
                if (helper == null) continue;
                if (helper.Channel == null)
                {
                    if (helper.ColliderName == "AttackRange") helper.Channel = context.attackChannel;
                    else if (helper.ColliderName == "SwiftDashRange") helper.Channel = context.swiftDashChannel;
                }
            }
        }
    }

    public void AdvanceCombo()
    {
        if (Time.time - lastAttackTime > config.ComboResetTime || currentComboCount >= config.MaxComboCount)
        {
            currentComboCount = 0;
        }

        if (currentComboCount < config.MaxComboCount)
        {
            var targetClip = config.ComboAttackAnimations[currentComboCount];
            if (targetClip != null && context.overrideController["Attack"] != targetClip)
            {
                context.overrideController["Attack"] = targetClip;
            }

            context.playerAnimator.SetInteger("Combo", currentComboCount);
            context.playerAnimator.SetBool("Attack", true);

            currentComboCount++;
            lastAttackTime = Time.time;
        }
    }

    public void HandleAttackRelease()
    {
        attackHoldTime = Time.time - attackHoldTime;
        if (attackHoldTime >= config.ChargedAttackWindUpDuration)
        {
            context.overrideController["Attack"] = config.ChargedAttackAnimation;
            context.playerAnimator.SetBool("Attack", true);
            context.playerAnimator.SetInteger("Combo", 0); // Reset combo count for charged attack
            currentComboCount = 0; // Reset combo count after charged attack
        }
        else
        {
            bool isAttacking = context.playerAnimator.GetBool("Attack");
            if (context.canCancel || !isAttacking)
            {
                AdvanceCombo();
                if (isAttacking)
                {
                    Debug.Log("Attack input received during an ongoing attack. Triggering AttackTransition.");
                    context.playerAnimator.SetTrigger("AttackTransition");
                }
            }
        }
        attackHoldTime = 0f;
    }

    public void HandleAttackInput()
    {
        attackHoldTime = Time.time;
    }

    public void HandleBlockInput()
    {
        context.playerAnimator.SetBool("Block", true);
    }

    public void HandleBlockRelease()
    {
        context.playerAnimator.SetBool("Block", false);
    }

    public void HandleGettingHit()
    {
        var animatorState = context.playerAnimator.GetCurrentAnimatorStateInfo(0);
        float elapsedBlockTime = animatorState.normalizedTime * animatorState.length;
        if (animatorState.IsName("Block") && elapsedBlockTime <= config.ParryTimeWindow)
        {
            // Restore time and negate damage
            context.playerAnimator.SetTrigger("Parry");
            Debug.Log("Player parried the attack!");
        }
        else
        {
            // Negate damage but no timer restoration
            Debug.Log("Player blocked the attack!");
        }
    }

    public void Update()
    {
        Vector2 playerPos = context.playerRigidbody != null
            ? context.playerRigidbody.position
            : (Vector2)context.playerAnimator.transform.position;

        Vector2 playerDirection = GetPlayerDirection != null ? GetPlayerDirection() : Vector2.right;

        if (context.attackChannel != null)
        {
            context.attackChannel.UpdateTargets(playerPos, playerDirection);
        }

        if (context.swiftDashChannel != null)
        {
            context.swiftDashChannel.UpdateTargets(playerPos, playerDirection);
        }
    }
}