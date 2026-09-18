using UnityEngine;

public class MusketeerBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 16.0f;
    [SerializeField] private float damage = 1.0f;
    [SerializeField] private float maxLifetime = 3.0f;
    private Rigidbody2D rb;
    public void Setup(Vector2 direction)
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Zero gravity
            rb.gravityScale = 0f; 
            rb.linearVelocity = direction.normalized * speed;
        }
        // Rotate the bullet to visually point in the travel direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        Destroy(gameObject, maxLifetime);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Hit Player
        var damageable = collision.GetComponent<IEntity>();
        var isPlayer = collision.GetComponent<PlayerController>();
        if (damageable != null && isPlayer != null)
        {
            damageable.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }
        // 2. Hit Ground or Wall
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
        }
    }
}