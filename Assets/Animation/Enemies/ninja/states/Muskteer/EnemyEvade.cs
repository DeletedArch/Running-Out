using UnityEngine;

public class EnemyEvade : MonoBehaviour
{
    [Header("Evade Cooldown")]
    [SerializeField] private float evadeCooldown = 3.0f;

    [Header("Trigger Settings")]
    [SerializeField] private float dangerDistance = 2.8f;

    private EnemyController controller;
    private Animator animator;
    private float cooldownTimer = 0f;

    private void Awake()
    {
        controller = GetComponent<EnemyController>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;

        if (controller == null || controller.IsDead)
            return;

        Transform target = controller.Context?.Target ?? controller.Context?.perception?.CurrentTarget;
        if (target == null)
            return;

        float distance = Vector2.Distance(transform.position, target.position);

        // Update proximity bool
        bool isPlayerClose = distance < dangerDistance;
        animator.SetBool("PlayerClose", isPlayerClose);

        // Turn Evade ON only when close AND cooldown is ready
        if (isPlayerClose && cooldownTimer <= 0f)
        {
            TriggerEvade();
        }
    }

    private void TriggerEvade()
    {
        cooldownTimer = evadeCooldown;
        animator.SetBool("Evade", true);
    }
}