using UnityEngine;
using System;

[RequireComponent(typeof(CircleCollider2D))]
public class RangeDetectionHelper : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask detectionLayer;
    [SerializeField] private string colliderName = "AttackRange";

    public event Action<Collider2D, string> OnObjectDetected;
    public event Action<Collider2D, string> OnObjectExited;

    void Awake()
    {
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            circleCollider.isTrigger = true;
            circleCollider.radius = detectionRadius;
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & detectionLayer) != 0)
        {
            OnObjectDetected?.Invoke(other, colliderName);
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & detectionLayer) != 0)
        {
            OnObjectExited?.Invoke(other, colliderName);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}