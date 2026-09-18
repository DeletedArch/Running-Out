using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    [SerializeField] private float damage = 0.5f;

    // Stores all entities hit during the current attack swing
    private HashSet<IEntity> hitEntities = new HashSet<IEntity>();
    private Collider2D col;
    private bool wasColliderEnabled;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        wasColliderEnabled = col != null && col.enabled;
    }

    private void OnEnable()
    {
        ResetHitbox();
    }

    private void OnDisable()
    {
        ResetHitbox();
    }

    private void Update()
    {
        // a new attack swing has started. Automatically clear the hit list.
        if (col != null)
        {
            if (col.enabled && !wasColliderEnabled)
            {
                ResetHitbox();
            }
            wasColliderEnabled = col.enabled;
        }
    }

    public void ResetHitbox()
    {
        hitEntities.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var damageable = collision.GetComponent<IEntity>();
        var isPlayer = collision.GetComponent<PlayerController>();

        if (hitEntities.Contains(damageable))
        {
            return;
        }

        hitEntities.Add(damageable);
        isPlayer.TakeDamage(damage, gameObject);
        Debug.Log($"Enemy sword hit {isPlayer.name} once for {damage} damage! (First contact: {collision.name})");
    }
}

