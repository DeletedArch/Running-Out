using System;
using UnityEngine;

[Serializable]
public class EnemyContext
{
    [Header("References")]
    public Rigidbody2D rb;
    public Animator animator;
    public EnemyPerception perception;
    public EnemyMovement enemyMovement;

    [Header("Layers")]
    public LayerMask playerLayer;
    public LayerMask groundLayer;
    public LayerMask enemyLayer;

    [Header("Interfaces / Edge")]
    public MonoBehaviour edgeResponseBehaviour;
    public IEdgeResponse edgeResponse => edgeResponseBehaviour as IEdgeResponse;

    [Header("Dynamic State")]
    public Transform Target { get; set; }
}