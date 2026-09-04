using UnityEngine;

public interface IEntity
{
    float Health { get; }

    void TakeDamage(float amount);
    
    void Die();
}