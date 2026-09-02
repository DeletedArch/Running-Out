using UnityEngine;
using System;

[RequireComponent(typeof(CircleCollider2D))]
public class RangeDetectionHelper : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask detectionLayer;

    public event Action<Collider2D> OnObjectDetected;
    public event Action<Collider2D> OnObjectExited;

    void Awake()
    {
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            circleCollider.isTrigger = true;
            circleCollider.radius = detectionRadius;
        }
    }

    private void Update()
    {
        
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & detectionLayer) != 0)
        {
            OnObjectDetected?.Invoke(other);
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & detectionLayer) != 0)
        {
            OnObjectExited?.Invoke(other);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}