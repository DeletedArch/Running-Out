using System;
using UnityEngine;


public class PlayerMovementConfig
{
    [SerializeField] internal float runSpeed = 5f;
    [SerializeField] internal float jumpForce = 3f;
    [SerializeField] internal float dashForce = 7f;
    [SerializeField] internal float wallHopForce = 2f;
    [SerializeField] internal float maxSpeed = 7f;
    [SerializeField] internal float maxSwiftDashDistance = 15f;
}

public class PlayerCombatConfig
{
    [SerializeField] internal float attackDuration = 0.33f;
    [SerializeField] internal float blockDuration = 0.3f;
    [SerializeField] internal float swiftDashDuration = 0.25f;
    [SerializeField] internal float chargedAttackWindUpDuration = 0.5f;
}

public class PlayerContext
{
    [Header("References")]
    [SerializeField] internal Rigidbody2D playerRigidbody;
    [SerializeField] internal Animator playerAnimator;
    [SerializeField] internal readonly PlayerMovementConfig playerMovementConfig;
    [SerializeField] internal readonly PlayerCombatConfig playerCombatConfig;
    
}