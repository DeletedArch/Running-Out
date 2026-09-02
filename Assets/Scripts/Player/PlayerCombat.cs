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
    private RangeDetectionHelper rangeDetectionHelper;
    private List<GameObject> enemiesInRange = new List<GameObject>();

    public delegate Vector2 GetPlayerDirectionDelegate();
    public GetPlayerDirectionDelegate GetPlayerDirection;

    public PlayerCombat(PlayerContext context, RangeDetectionHelper rangeDetectionHelper)
    {
        this.config = context.playerCombatConfig;
        this.context = context;
        this.rangeDetectionHelper = rangeDetectionHelper;
        this.rangeDetectionHelper.OnObjectDetected += HandleRangeDetection;
        this.rangeDetectionHelper.OnObjectExited += HandleRangeExit;
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
            if (context.canCancel)
            {
                AdvanceCombo();
                context.playerAnimator.Play("Attack", 0, 0f);
            }
            else if (!context.playerAnimator.GetBool("Attack"))
            {
                AdvanceCombo();
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
        if (animatorState.IsName("Block") && animatorState.normalizedTime < config.ParryTimeWindow)
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

    void HandleRangeDetection(Collider2D detectedObject)
    {
        enemiesInRange.Add(detectedObject.gameObject);
        SetDetectedEnemy();
    }

    void SetDetectedEnemy()
    {
        float closestDistance = Mathf.Infinity;
        Vector2 playerDirection = GetPlayerDirection();
        for (int i = enemiesInRange.Count - 1; i >= 0; i--)
        {
            if (enemiesInRange[i] == null)
            {
                enemiesInRange.RemoveAt(i);
                continue;
            }
            float distance = Vector2.Distance(rangeDetectionHelper.transform.position, enemiesInRange[i].transform.position);
            Vector2 directionToEnemy = (enemiesInRange[i].transform.position - rangeDetectionHelper.transform.position).normalized;
            float dotProduct = Vector2.Dot(directionToEnemy, playerDirection); // This will give a value between -1 and 1, where 1 means the enemy is directly in front of the player
            if (distance < closestDistance && dotProduct > 0.5f)
            {
                closestDistance = distance;
                context.detectedEnemy = enemiesInRange[i];
            }
            else if (distance < closestDistance && dotProduct > 0f)
            {
                context.detectedEnemy2 = enemiesInRange[i];
            }
        }
    }

    void HandleRangeExit(Collider2D exitedObject)
    {
        enemiesInRange.Remove(exitedObject.gameObject);
        if (context.detectedEnemy == exitedObject.gameObject)
        {
            context.detectedEnemy = null;
        }
        else if (context.detectedEnemy2 == exitedObject.gameObject)
        {
            context.detectedEnemy2 = null;
        }
        SetDetectedEnemy();
    }
}