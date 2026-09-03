using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class SwiftDashSMB : StateMachineBehaviour
{
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private float maxLungeDistance = 3f;
    [SerializeField] private float lungeDuration = 15f;



    private Vector2 startPosition;
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private PlayerMovementConfig movementConfig;
    private float dashDirection;
    private float elapsedTime;
    private float maxDashDuration;
    private float originalGravityScale = 1f;
    private bool isSuspended = false;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var player = animator.GetComponent<PlayerController>();
        if (player == null) return;

        rb = player.Context.playerRigidbody;
        playerCollider = player.GetComponent<Collider2D>();
        movementConfig = player.Context.playerMovementConfig;

        elapsedTime = 0f;

        maxDashDuration = (movementConfig.DashDistance / movementConfig.DashForce) + 0.1f;
        originalGravityScale = rb.gravityScale;
        isSuspended = false;
        ApplyTargetedImpulse(player);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (rb == null || movementConfig == null) return;

        if (isSuspended && rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }

        elapsedTime += Time.deltaTime;

        Vector2 checkOrigin = playerCollider != null ? (Vector2)playerCollider.bounds.center : rb.position;
        Vector2 checkDirection = new Vector2(dashDirection, 0f);

        RaycastHit2D wallHit = Physics2D.Raycast(checkOrigin, checkDirection, wallCheckDistance, wallMask);

        Debug.DrawRay(checkOrigin, checkDirection * wallCheckDistance, Color.darkOrange);

        if (wallHit.collider != null)
        {
            animator.SetBool("Dash", false);
            return;
        }

        float distanceTraveled = Vector2.Distance(startPosition, rb.position);
        if (distanceTraveled >= movementConfig.DashDistance || elapsedTime >= maxDashDuration)
        {
            animator.SetBool("Dash", false);
            return;
        }

        rb.linearVelocity = new Vector2(dashDirection * movementConfig.DashForce, 0);
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
        }).ContinueWith(() =>
        {
            var enemy = playerTransform.GetComponent<PlayerController>().Context.detectedEnemySwiftDash[0];
            if (enemy != null)
            {
                Destroy(enemy);
                playerTransform.GetComponent<PlayerController>().Context.detectedEnemySwiftDash.Remove(enemy);
            }
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
        List<GameObject> detectedEnemies = player.Context.detectedEnemySwiftDash;
        if (detectedEnemies == null || detectedEnemies.Count == 0)
        {
            return null;
        }
        GameObject targetEnemy = player.Context.detectedEnemySwiftDash[0];
        if (targetEnemy == null)
        {
            return null;
        }
        float distanceToEnemy = Vector2.Distance(player.transform.position, targetEnemy.transform.position);
        if (distanceToEnemy > maxLungeDistance)
        {
            return null;
        }
        return (Vector2)targetEnemy.transform.position;
    }
}