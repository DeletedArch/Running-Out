using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

public class SwiftDashSMB : StateMachineBehaviour, ITimerAccess
{
    [SerializeField] private float maxLungeDistance = 15f;
    [SerializeField] private float lungeDuration = 0.25f;
    [SerializeField] private TargetDetectionChannel channel;
    [SerializeField] private float timerUsage = 1f;
    [SerializeField] private float timerRestoration = 2f;
    [SerializeField] private float hitstopDuration = 0.25f;
    [SerializeField] private float shakeIntensity = 0.5f;

    public float TimerUsage => timerUsage;
    public float TimerRestoration => timerRestoration;

    private CancellationTokenSource stateCts;
    private PlayerController player;
    private Rigidbody2D rb;
    private float originalGravityScale = 1f;
    private bool isSuspended = false;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        stateCts?.Cancel();
        stateCts?.Dispose();
        stateCts = new CancellationTokenSource();

        var player = animator.GetComponent<PlayerController>();
        if (player == null) return;
        this.player = player;
        rb = player.Context.playerRigidbody;
        player.UseCooldown("SwiftDash");
        originalGravityScale = rb.gravityScale;
        isSuspended = false;
        ITimerAccess.ModifyTimer(-timerUsage); // Deduct timer usage when the dash starts
        ApplyTargetedImpulse(player, animator);
        rb.excludeLayers = player.Context.enemyLayer;
        player.Context.playerCombatConfig.SwiftDashSound?.Play(rb.position);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (rb == null) return;

        if (isSuspended && rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
    }

    private void ApplyTargetedImpulse(PlayerController player, Animator animator)
    {
        Rigidbody2D playerRb = player.Context.playerRigidbody;
        TargetData targetData = GetTargetedEnemyData(player);
        if (targetData != null && targetData.Object != null)
        {
            isSuspended = true;
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
            }

            Vector2 targetedEnemyPosition = (Vector2)targetData.Object.transform.position;
            float enemyX = targetedEnemyPosition.x;
            float playerX = player.transform.position.x;
            float diffX = enemyX - playerX;
            Vector2 direction = targetedEnemyPosition - (Vector2)player.transform.position;

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
            Vector2 adjustedTargetPosition = new Vector2(targetX, targetedEnemyPosition.y) + direction.normalized * 5f;
            Vector2 hitTargetPosition = new Vector2(targetX, targetedEnemyPosition.y);
            ApplyAlphaImpulse(playerRb, player.transform, adjustedTargetPosition, hitTargetPosition, lungeDuration, animator, targetData.Object, stateCts.Token).Forget();
        }
        else
        {
            animator.SetBool("SDash", false);
        }
    }

    private async UniTaskVoid ApplyAlphaImpulse(Rigidbody2D targetRb, Transform playerTransform, Vector2 endPosition, Vector2 hitPosition, float duration, Animator animator, GameObject targetedEnemy, CancellationToken ct)
    {
        Vector2 startPosition = playerTransform.position;
        float elapsedTime = 0f;
        float timer = Mathf.Max(0.1f, animator.GetFloat("Timer"));
        duration = lungeDuration / timer / 2f;
        bool hasHitEnemy = false;

        try
        {
            await UniTask.WaitUntil(() =>
            {
                elapsedTime += Time.fixedDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                Vector2 newPosition = Vector2.Lerp(startPosition, hitPosition, t);
                targetRb.MovePosition(newPosition);
                return elapsedTime >= duration;
            }, PlayerLoopTiming.FixedUpdate, ct);

            // Ensure the final position is set to the exact end position
            // playerTransform.position = endPosition;
            Quaternion rotation = Quaternion.LookRotation(hitPosition - startPosition, Vector3.up);
            rotation.y = 0f; // Ensure the rotation is only around the Y-axis
            rotation.x = 0f;
            VFXEvents.TriggerVFX("SwiftDash", hitPosition, rotation);

            if (targetedEnemy != null || !hasHitEnemy)
            {
                var damageable = targetedEnemy.GetComponent<IEntity>();
                if (damageable != null)
                {
                    VFXEvents.TriggerVFX("Hit", hitPosition, Quaternion.identity);
                    damageable.TakeDamage(player.Context.playerCombatConfig.SwiftDashDamage);
                    hasHitEnemy = true;
                    ITimerAccess.ModifyTimer(timerRestoration);
                    var enemyAnimator = targetedEnemy.GetComponent<Animator>();
                    player.impulseSource?.GenerateImpulseWithVelocity(Vector3.one * shakeIntensity);
                    await ActionHelpers.ApplyGlobalHitstop(hitstopDuration);
                }
            }
            await UniTask.WaitUntil(() =>
            {
                elapsedTime += Time.fixedDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                Vector2 newPosition = Vector2.Lerp(hitPosition, endPosition, t);
                targetRb.MovePosition(newPosition);
                return elapsedTime >= duration;
            }, PlayerLoopTiming.FixedUpdate, ct);
            animator.SetBool("SDash", false);
        }
        catch (System.OperationCanceledException)
        {
            if (hasHitEnemy) return; // Already hit the enemy, no need to apply damage again
            // Interrupted early (e.g. damaged, staggered, or transitioned out)
            Quaternion rotation = Quaternion.LookRotation(hitPosition - startPosition, Vector3.up);
            rotation.x = 0f;
            rotation.z = 0f;
            VFXEvents.TriggerVFX("SwiftDash", hitPosition, rotation);
            if (Vector2.Distance(playerTransform.position, endPosition) < 0.5f || Vector2.Distance(playerTransform.position, hitPosition) < 0.5f)
            {
                if (targetedEnemy != null)
                {
                    VFXEvents.TriggerVFX("Hit", hitPosition, Quaternion.identity);
                    var damageable = targetedEnemy.GetComponent<IEntity>();
                    if (damageable != null)
                    {
                        damageable.TakeDamage(player.Context.playerCombatConfig.SwiftDashDamage);
                        ITimerAccess.ModifyTimer(timerRestoration);
                        var enemyAnimator = targetedEnemy.GetComponent<Animator>();
                        player.impulseSource?.GenerateImpulseWithVelocity(Vector3.one * shakeIntensity);
                        await ActionHelpers.ApplyGlobalHitstop(hitstopDuration);
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
        if (rb != null)
        {
            rb.excludeLayers = 0;
        }
    }

    private TargetData GetTargetedEnemyData(PlayerController player)
    {
        var targetCh = channel != null ? channel : player.Context.swiftDashChannel;
        if (targetCh != null)
        {
            return targetCh.GetBestTarget(0f, maxLungeDistance);
        }
        return null;
    }
}