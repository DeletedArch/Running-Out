using System;
using UnityEngine;

//public enum EnemyState
//{
    //Idle,
    //patrol,
    //chase,
    //Aim,
    //Shoot,
    //Evade,
    //Run,
    //Attack,
    //Deflect,
    //Stun
    ////gonna add more in the future for the boss fight
//}

[Serializable]
public class EnemyContext : MonoBehaviour
{
    [Header("References")]
    [SerializeField] internal Rigidbody2D rb;
    [SerializeField] internal Animator animator;
    [SerializeField] internal EnemyPerception perception;
    [SerializeField] internal EnemyMovement enemyMovement;

    [Header("Layers")]
    [SerializeField] internal LayerMask playerLayer;
    [SerializeField] internal LayerMask groundLayer;

    //[Header("enemy state")]
    //[SerializeField] internal EnemyState currentPlayerState = EnemyState.Idle;

    [Header("for different cases")]
    [SerializeField] internal MonoBehaviour edgeResponseBehaviour;
    internal IEdgeResponse edgeResponse => edgeResponseBehaviour as IEdgeResponse;
}
