using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    [SerializeField] private float damage = 0.5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var damageable = collision.GetComponent<IEntity>();
        var isPlayer = collision.GetComponent<PlayerController>();
        if ( damageable != null && isPlayer != null)
        {
            isPlayer.TakeDamage(damage, gameObject);
            Debug.Log($"Enemy sword hit {collision.name} for {damage} damage!");
        }
    }
}

