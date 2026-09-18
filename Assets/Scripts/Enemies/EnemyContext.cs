using System;
using UnityEngine;

[Serializable]
public class EnemyContext
{
    [Header("References")]
    public Rigidbody2D rb;
    public Animator animator;
    public EnemyPerception perception;
    public SpriteRenderer spriteRenderer;
    public EnemyMovement enemyMovement;

    [Header("Layers")]
    public LayerMask playerLayer;
    public LayerMask groundLayer;
    public LayerMask enemyLayer;

    [Header("Interfaces / Edge")]
    public MonoBehaviour edgeResponseBehaviour;
    public IEdgeResponse edgeResponse => edgeResponseBehaviour as IEdgeResponse;

    [Header("Dynamic State")]
    public Vector2 AimLockedPosition { get; set; }
    public Vector2 EvadeStartPos { get; set; }
    public Transform Target { get; set; }
}