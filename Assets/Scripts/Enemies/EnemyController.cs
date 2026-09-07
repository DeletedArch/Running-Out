using UnityEngine;

public class EnemyController : MonoBehaviour, IEntity
{
    [Header("References")]
    [SerializeField] private EnemyContext context;
    private Animator animator;

    [Header("Stats")]
    [SerializeField] private float maxHealth = 70f;
    private float currentHealth;

    public EnemyContext Context => context;
    public float Health => currentHealth;
    public bool IsDead => currentHealth <= 0;

    private void Awake()
    {
        currentHealth = maxHealth;
        context.channel = GetComponentInChildren<RangeDetectionHelper>().Channel;

        if (animator == null)
            animator = context.animator;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} dmg. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
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