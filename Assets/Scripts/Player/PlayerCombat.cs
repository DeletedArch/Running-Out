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
    private Dictionary<string, List<GameObject>> enemiesInRange = new Dictionary<string, List<GameObject>>();

    public delegate Vector2 GetPlayerDirectionDelegate();
    public GetPlayerDirectionDelegate GetPlayerDirection;

    public PlayerCombat(PlayerContext context, RangeDetectionHelper[] rangeDetectionHelper)
    {
        this.config = context.playerCombatConfig;
        this.context = context;
        this.rangeDetectionHelper = rangeDetectionHelper;
        foreach (var helper in rangeDetectionHelper)
        {
            helper.OnObjectDetected += HandleRangeDetection;
            helper.OnObjectExited += HandleRangeExit;
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

    void HandleRangeDetection(Collider2D detectedObject, string colliderName)
    {
        if (detectedObject == null) return;
        GameObject enemyGo = detectedObject.gameObject;

        if (!enemiesInRange.ContainsKey(colliderName))
        {
            enemiesInRange[colliderName] = new List<GameObject>();
        }

        if (!enemiesInRange[colliderName].Contains(enemyGo))
        {
            enemiesInRange[colliderName].Add(enemyGo);
        }

        SetDetectedEnemy(colliderName);
    }

    void HandleRangeExit(Collider2D exitedObject, string colliderName)
    {
        if (exitedObject == null) return;
        GameObject enemyGo = exitedObject.gameObject;

        if (enemiesInRange.ContainsKey(colliderName))
        {
            enemiesInRange[colliderName].Remove(enemyGo);
        }

        SetDetectedEnemy(colliderName);
    }

    void SetDetectedEnemy(string colliderName)
    {
        if (context.detectedEnemy == null) context.detectedEnemy = new List<GameObject>();
        if (context.detectedEnemySwiftDash == null) context.detectedEnemySwiftDash = new List<GameObject>();

        if (!enemiesInRange.ContainsKey(colliderName)) return;

        List<GameObject> targetList;
        float minDotProduct;

        if (colliderName == "AttackRange")
        {
            targetList = context.detectedEnemy;
            minDotProduct = 0.5f;                                                                 
        }
        else if (colliderName == "SwiftDashRange")
        {
            targetList = context.detectedEnemySwiftDash;
            minDotProduct = 0f;                                                                  
        }
        else
        {
            return;
        }

        targetList.Clear();

        List<GameObject> enemies = enemiesInRange[colliderName];

        enemies.RemoveAll(e => e == null);

        Vector2 playerPos = context.playerRigidbody.position;

        Vector2 playerDirection = GetPlayerDirection();

        var validEnemies = new List<(GameObject enemy, float distance)>();

        foreach (var enemy in enemies)
        {
            Vector2 toEnemy = (Vector2)enemy.transform.position - playerPos;
            float distance = toEnemy.magnitude;
            Vector2 directionToEnemy = toEnemy / (distance > 0.0001f ? distance : 1f);

            float dotProduct = Vector2.Dot(directionToEnemy, playerDirection);

            if (dotProduct > minDotProduct)
            {
                validEnemies.Add((enemy, distance));
            }
        }

        validEnemies.Sort((a, b) => a.distance.CompareTo(b.distance));

        foreach (var item in validEnemies)
        {
            targetList.Add(item.enemy);
        }
    }
}