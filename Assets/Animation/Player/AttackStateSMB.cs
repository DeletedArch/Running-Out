using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

public class AttackImpulseSMB : StateMachineBehaviour, ITimerAccess
{
    [SerializeField] private float maxLungeDistance = 3f;
    [SerializeField] private float lungeDuration = 0.2f;
    [SerializeField] private TargetDetectionChannel channel;
    [SerializeField] private float timerUsage = 0.5f;
    [SerializeField] private float timerRestoration = 0.75f;

    public float TimerUsage => timerUsage;
    public float TimerRestoration => timerRestoration;

    private CancellationTokenSource stateCts;
    private Rigidbody2D rb;
    private float originalGravityScale = 1f;
    private bool isSuspended = false;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        stateCts?.Cancel();
        stateCts?.Dispose();
        stateCts = new CancellationTokenSource();

        animator.SetBool("Attack", true); // Ensure the Attack boolean is set to true when entering the state
        PlayerController player = animator.GetComponent<PlayerController>();
        if (player == null) return;

        rb = player.Context.playerRigidbody;
        if (rb != null)
        {
            originalGravityScale = rb.gravityScale;
        }
        isSuspended = false;
        ITimerAccess.ModifyTimer(-timerUsage);
        ApplyTargetedImpulse(player);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (isSuspended && rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }

        if (stateInfo.normalizedTime >= 1f && !animator.IsInTransition(layerIndex))
        {
            animator.SetBool("Attack", false);
        }
    }

    private void ApplyTargetedImpulse(PlayerController player)
    {
        Rigidbody2D playerRb = player.Context.playerRigidbody;
        Vector2? targetedEnemyPosition = GetTargetedEnemy(player);
        if (targetedEnemyPosition != null && targetedEnemyPosition.HasValue)
        {
            isSuspended = true;
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
            }

            float enemyX = targetedEnemyPosition.Value.x;                                                                
            float playerX = player.transform.position.x;                                                                 
            float diffX = enemyX - playerX;                                                                              
                                                                                                                         
            // 1. ALWAYS face the enemy directly (only if not on the exact same X)                                       
            if (Mathf.Abs(diffX) > 0.05f)                                                                                
            {                                                                                                            
                float facingDir = Mathf.Sign(diffX);                                                                     
                player.transform.localScale = new Vector3(                                                               
                    facingDir * Mathf.Abs(player.transform.localScale.x),                                                
                    player.transform.localScale.y,                                                                       
                    player.transform.localScale.z                                                                        
                );                                                                                                       
            }                                                                                                            
            float currentDistance = Mathf.Abs(diffX);                                                                    
            float stoppingGap = Mathf.Min(1.0f, currentDistance * 0.5f);    
            float targetX = enemyX - (Mathf.Sign(diffX) * stoppingGap);                                                  
            Vector2 adjustedTargetPosition = new Vector2(targetX, targetedEnemyPosition.Value.y);
            GameObject targetedEnemy = GetTargetedEnemyObject(player);
            ApplyAlphaImpulse(playerRb, player.transform, adjustedTargetPosition, lungeDuration, player.Context.playerAnimator, player, stateCts.Token, targetedEnemy).Forget();
        }
    }

    private async UniTaskVoid ApplyAlphaImpulse(Rigidbody2D targetRb, Transform playerTransform, Vector2 endPosition, float duration, Animator animator, PlayerController player, CancellationToken ct, GameObject targetedEnemy)
    {
        Vector2 startPosition = playerTransform.position;
        float elapsedTime = 0f;

        try
        {
            await UniTask.WaitUntil(() =>
            {
                duration = lungeDuration / animator.GetFloat("Timer");
                elapsedTime += Time.fixedDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                Vector2 newPosition = Vector2.Lerp(startPosition, endPosition, t);
                targetRb.MovePosition(newPosition);
                return elapsedTime >= duration;
            }, PlayerLoopTiming.FixedUpdate, ct);

            Debug.Log("AttackImpulseSMB: Player reached the target position.");

            // Damage the enemy
            if (targetedEnemy != null)
            {
                Debug.Log("AttackImpulseSMB: Attempting to damage the enemy.");
                var damageable = targetedEnemy.GetComponent<IEntity>();
                if (damageable != null)
                {
                    Debug.Log("AttackImpulseSMB: Damaging the enemy.");
                    damageable.TakeDamage(player.Context.playerCombatConfig.AttackDamage);
                    ITimerAccess.ModifyTimer(timerRestoration);
                }
            }
        }
        catch (OperationCanceledException)
        {
            if (Vector2.Distance(playerTransform.position, endPosition) < 0.5f)
            {
                Debug.Log("AttackImpulseSMB: Player reached the target position before cancellation.");
                if (targetedEnemy != null)
                {
                    Debug.Log("AttackImpulseSMB: Attempting to damage the enemy after cancellation.");
                    var damageable = targetedEnemy.GetComponent<IEntity>();
                    if (damageable != null)
                    {
                        Debug.Log("AttackImpulseSMB: Damaging the enemy after cancellation.");
                        damageable.TakeDamage(player.Context.playerCombatConfig.SwiftDashDamage);
                        ITimerAccess.ModifyTimer(timerRestoration);
                    }
                }
            }
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        stateCts?.Cancel();
        stateCts?.Dispose();
        stateCts = null;

        if (isSuspended && rb != null)
        {
            rb.gravityScale = originalGravityScale;
            isSuspended = false;
        }
    }

    private Vector2? GetTargetedEnemy(PlayerController player)
    {
        var targetCh = channel != null ? channel : player.Context.attackChannel;
        if (targetCh != null)
        {
            var best = targetCh.GetBestTarget(0.3f, maxLungeDistance);
            if (best != null && best.Object != null)
            {
                return (Vector2)best.Object.transform.position;
            }
            return null;
        }
        return null;
    }

    private GameObject GetTargetedEnemyObject(PlayerController player)
    {
        var targetCh = channel != null ? channel : player.Context.attackChannel;
        if (targetCh != null)
        {
            var best = targetCh.GetBestTarget(0.3f, maxLungeDistance);
            if (best != null && best.Object != null)
            {
                return best.Object;
            }
            return null;
        }
        return null;
    }
}