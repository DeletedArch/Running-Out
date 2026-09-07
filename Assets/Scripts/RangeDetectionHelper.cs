using UnityEngine;
using System;

[RequireComponent(typeof(CircleCollider2D))]
public class RangeDetectionHelper : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask detectionLayer;
    [SerializeField] private string colliderName = "AttackRange";
    [SerializeField] private TargetDetectionChannel channel;

    public TargetDetectionChannel Channel { get => channel; set => channel = value; }
    public string ColliderName => colliderName;

    public event Action<Collider2D, string> OnObjectDetected;
    public event Action<Collider2D, string> OnObjectExited;

    void Awake()
    {
        if (channel == null)
        {
            channel = ScriptableObject.CreateInstance<TargetDetectionChannel>();
        }

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
            channel?.Add(other.gameObject);
            OnObjectDetected?.Invoke(other, colliderName);
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & detectionLayer) != 0)
        {
            channel?.Remove(other.gameObject);
            OnObjectExited?.Invoke(other, colliderName);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}