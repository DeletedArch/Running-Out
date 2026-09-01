using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public class PlayerCombat
{
    private PlayerCombatConfig config;
    private PlayerContext context;

    private int currentComboCount = 0;
    private float lastAttackTime = 0f;
    private float attackHoldTime = 0f;

    public PlayerCombat(PlayerContext context)
    {
        this.config = context.playerCombatConfig;
        this.context = context;
    }

    public void AdvanceCombo()
    {
        if (Time.time - lastAttackTime > config.ComboResetTime || currentComboCount >= config.MaxComboCount)
        {
            currentComboCount = 0;
        }

        if (currentComboCount < config.MaxComboCount)
        {
            context.overrideController["Attack"] = config.ComboAttackAnimations[currentComboCount];
            context.playerAnimator.SetBool("Attack", true);
            UniTask.Delay(TimeSpan.FromSeconds(config.ComboAttackDuration)).ContinueWith(() =>
            {
                context.playerAnimator.SetBool("Attack", false);
            }).Forget();
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
            UniTask.Delay(TimeSpan.FromSeconds(config.ComboAttackDuration)).ContinueWith(() =>
            {
                context.playerAnimator.SetBool("Attack", false);
            }).Forget();
        }
        else
        {
            AdvanceCombo();
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
        UniTask.Delay(TimeSpan.FromSeconds(config.BlockDuration)).ContinueWith(() =>
        {
            context.playerAnimator.SetBool("Block", false);
        }).Forget();
    }

    public void HandleGettingHit()
    {
        var animatorState = context.playerAnimator.GetCurrentAnimatorStateInfo(0);
        if (animatorState.IsName("Block") && animatorState.normalizedTime < config.ParryTimeWindow)
        {
            // Player is blocking, reduce damage or negate it
            Debug.Log("Player parried the attack!");
        }
        else
        {
            // Player takes full damage
            Debug.Log("Player got hit!");
            // context.playerAnimator.SetTrigger("Hit");
        }
    }

    void GotoEnemy()
    {
        // Implement logic to move the player towards the enemy
    }

    void FindNearestEnemy(Vector2 moveInput)
    {
        // Implement logic to find the nearest enemy
    }


}