using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerContext
{
    [Header("References")]
    [SerializeField] internal Rigidbody2D playerRigidbody;
    [SerializeField] internal Animator playerAnimator;
    [SerializeField] internal AnimatorOverrideController overrideController;
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
    
    [Header("Runtime Settings")]
    [SerializeField] internal bool canMove = true;
    [SerializeField] internal bool isInvincible = false;
    [SerializeField] internal bool canCancel = true;

    [Header("State")]
    [SerializeField] internal string currentState;
    [SerializeField] internal Vector2 moveInput;

    [Header("Enemy Detection Channels")]
    [SerializeField] internal TargetDetectionChannel attackChannel;
    [SerializeField] internal TargetDetectionChannel swiftDashChannel;
}