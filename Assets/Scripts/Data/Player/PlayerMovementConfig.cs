using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementConfig", menuName = "Configs/PlayerMovementConfig")]
public class PlayerMovementConfig : ScriptableObject
{
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float jumpForce = 3f;
    [SerializeField] private float dashForce = 10f;
    [SerializeField] private float dashDistance = 7f;
    [SerializeField] private float wallHopForce = 2f;
    [SerializeField] private float maxSpeed = 7f;
    [SerializeField] private float maxSwiftDashDistance = 15f;

    public float RunSpeed { get { return runSpeed; } }
    public float JumpForce { get { return jumpForce; } }
    public float DashForce { get { return dashForce; } }
    public float DashDistance { get { return dashDistance; } }
    public float WallHopForce { get { return wallHopForce; } }
    public float MaxSpeed { get { return maxSpeed; } }
    public float MaxSwiftDashDistance { get { return maxSwiftDashDistance; } }
}
