using UnityEngine;

public class EnemyController : MonoBehaviour, IEntity
{
    [Header("References")]
    [SerializeField] private EnemyContext context;
    private Animator animator;

    [Header("Stats")]
    [SerializeField] private float maxHealth = 70f;
    private float currentHealth;

    [Header("Audio")]
    [SerializeField] private SoundData fleshHitSound;    
    [SerializeField] private SoundData deathSound;
    [SerializeField] private SoundData hurtVoiceSound; 
    public EnemyContext Context => context;
    public float Health => currentHealth;
    public bool IsDead => currentHealth <= 0;

    private void Awake()
    {
        currentHealth = maxHealth;

        // Auto-wire context components if not assigned in Inspector
        if (context.rb == null) context.rb = GetComponent<Rigidbody2D>();
        if (context.enemyMovement == null) context.enemyMovement = GetComponent<EnemyMovement>();
        if (context.perception == null) context.perception = GetComponent<EnemyPerception>();
        if (context.edgeResponseBehaviour == null) context.edgeResponseBehaviour = GetComponent<OnEdgeResponse>();
        if (context.animator == null) context.animator = GetComponentInChildren<Animator>();

        animator = context.animator;
    }


    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} dmg. HP: {currentHealth}/{maxHealth}");
        // 1. ALWAYS play the bone breaking & blood shed sound on impact:
        fleshHitSound?.Play(transform.position);
        if (currentHealth <= 0)
        {
            // 2. Play death sound if they die:
            deathSound?.Play(transform.position);
            Die();
        }
        else
        {
            animator.SetTrigger("isHit");
        }
    }

    public void Die()
    {
        animator.SetBool("isDead", true);
        animator.Play("Die", 0, 0f);
    }
}