using System;
using UnityEngine;

public enum PlayerState
{
    Idle,
    Running,
    Jumping,
    Falling,
    Dashing,
    SwiftDashing,
    Attacking,
    Blocking,
    WallHopping
}

[Serializable]
public class PlayerContext
{
    [Header("References")]
    [SerializeField] internal Rigidbody2D playerRigidbody;
    [SerializeField] internal Animator playerAnimator;
    [SerializeField] internal PlayerMovementConfig playerMovementConfig;
    [SerializeField] internal PlayerCombatConfig playerCombatConfig;

    [Header("Layers")]
    [SerializeField] internal LayerMask groundLayer;
    [SerializeField] internal LayerMask wallLayer;
    [SerializeField] internal LayerMask wallHopLayer;
    [SerializeField] internal LayerMask enemyLayer;

    [Header("Settings")]
    [SerializeField] internal bool useDrag;
    [SerializeField] internal float customDrag = 1f;

    [Header("State")]
    [SerializeField] internal Vector2 moveInput;
    [SerializeField] internal PlayerState currentPlayerState = PlayerState.Idle;
}