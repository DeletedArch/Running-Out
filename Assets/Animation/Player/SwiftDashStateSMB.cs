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

    public float TimerUsage => timerUsage;
    public float TimerRestoration => timerRestoration;
    public static event System.Action<float> OnTimerChange;

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

        originalGravityScale = rb.gravityScale;
        isSuspended = false;
        ITimerAccess.ModifyTimer(-timerUsage); // Deduct timer usage when the dash starts
        ApplyTargetedImpulse(player, animator);
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
            Vector2 adjustedTargetPosition = new Vector2(targetX, targetedEnemyPosition.y);
            ApplyAlphaImpulse(playerRb, player.transform, adjustedTargetPosition, lungeDuration, animator, targetData.Object, stateCts.Token).Forget();
        } else
        {
            animator.SetBool("SDash", false);
        }
    }

    private async UniTaskVoid ApplyAlphaImpulse(Rigidbody2D targetRb, Transform playerTransform, Vector2 endPosition, float duration, Animator animator, GameObject targetedEnemy, CancellationToken ct)
    {
        Vector2 startPosition = playerTransform.position;
        float elapsedTime = 0f;
        float timer = Mathf.Max(0.1f, animator.GetFloat("Timer"));
        duration = lungeDuration / timer;

        try
        {
            await UniTask.WaitUntil(() =>
            {
                elapsedTime += Time.fixedDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / duration);
                Vector2 newPosition = Vector2.Lerp(startPosition, endPosition, t);
                targetRb.MovePosition(newPosition);
                return elapsedTime >= duration;
            }, PlayerLoopTiming.FixedUpdate, ct);

            // Ensure the final position is set to the exact end position
            targetRb.MovePosition(endPosition);
            playerTransform.position = endPosition;

            if (targetedEnemy != null)
            {
                var damageable = targetedEnemy.GetComponent<IEntity>();
                if (damageable != null)
                {
                    damageable.TakeDamage(player.Context.playerCombatConfig.SwiftDashDamage);
                    ITimerAccess.ModifyTimer(timerRestoration);
                }
            }
            animator.SetBool("SDash", false);
        }
        catch (System.OperationCanceledException)
        {
            // Interrupted early (e.g. damaged, staggered, or transitioned out)
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