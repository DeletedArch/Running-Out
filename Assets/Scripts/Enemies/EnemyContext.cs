using System;
using UnityEngine;


[Serializable]
public class EnemyContext 
{
    [Header("References")]
    [SerializeField] internal Rigidbody2D rb;
    [SerializeField] internal Animator animator;
    [SerializeField] internal TargetDetectionChannel channel;
    [SerializeField] internal EnemyPerception perception;
    [SerializeField] internal EnemyMovement enemyMovement;
    [Header("Layers")]
    [SerializeField] internal LayerMask playerLayer;
    [SerializeField] internal LayerMask groundLayer;
    [Header("Interfaces / Edge")]
    [SerializeField] internal MonoBehaviour edgeResponseBehaviour;
    internal IEdgeResponse edgeResponse => edgeResponseBehaviour as IEdgeResponse;
}
