using UnityEngine;

[CreateAssetMenu(fileName = "PlayerCombatConfig", menuName = "Configs/PlayerCombatConfig")]
public class PlayerCombatConfig : ScriptableObject
{
    [SerializeField] public float attackDuration = 0.33f;
    [SerializeField] public float blockDuration = 0.3f;
    [SerializeField] public float swiftDashDuration = 0.25f;
    [SerializeField] public float chargedAttackWindUpDuration = 0.5f;

    public float AttackDuration { get { return attackDuration; } }
    public float BlockDuration { get { return blockDuration; } }
    public float SwiftDashDuration { get { return swiftDashDuration; } }
    public float ChargedAttackWindUpDuration { get { return chargedAttackWindUpDuration; } }
}
