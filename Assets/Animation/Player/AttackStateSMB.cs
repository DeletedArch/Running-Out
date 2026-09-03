using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AttackImpulseSMB : StateMachineBehaviour
{
    [SerializeField] private float maxLungeDistance = 3f;
    [SerializeField] private float lungeDuration = 0.2f;
    [SerializeField] private TargetDetectionChannel channel;

    private Rigidbody2D rb;
    private float originalGravityScale = 1f;
    private bool isSuspended = false;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Attack", true); // Ensure the Attack boolean is set to true when entering the state
        PlayerController player = animator.GetComponent<PlayerController>();
        if (player == null) return;

        rb = player.Context.playerRigidbody;
        if (rb != null)
        {
            originalGravityScale = rb.gravityScale;
        }
        isSuspended = false;
        
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
            ApplyAlphaImpulse(playerRb, player.transform, adjustedTargetPosition, lungeDuration).Forget();
        }
    }

    private UniTask ApplyAlphaImpulse(Rigidbody2D targetRb, Transform playerTransform, Vector2 endPosition, float duration)
    {
        Vector2 startPosition = playerTransform.position;
        float elapsedTime = 0f;

        return UniTask.WaitUntil(() =>
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            Vector2 newPosition = Vector2.Lerp(startPosition, endPosition, t);
            targetRb.MovePosition(newPosition);
            return elapsedTime >= duration;
        });
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
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
}