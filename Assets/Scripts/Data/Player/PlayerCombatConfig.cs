using UnityEngine;

[CreateAssetMenu(fileName = "PlayerCombatConfig", menuName = "Configs/PlayerCombatConfig")]
public class PlayerCombatConfig : ScriptableObject
{
    [Header("Combat Settings")]
    [SerializeField] private float attackDuration = 0.33f;
    [SerializeField] private float blockDuration = 0.3f;
    [SerializeField] private float parryTimeWindow = 0.2f;
    [SerializeField] private float swiftDashDuration = 0.25f;
    [SerializeField] private float chargedAttackWindUpDuration = 0.5f;
    [SerializeField] private float attackDamage = 30f;
    [SerializeField] private float chargedAttackDamage = 150f;
    [SerializeField] private float swiftDashDamage = 70f;

    [Header("Combo Settings")]
    [SerializeField] private float comboResetTime = 1.0f;
    [SerializeField] private float comboInputWindow = 0.5f;
    [SerializeField] private float comboAttackDuration = 0.25f;
    [SerializeField] private AnimationClip[] comboAttackAnimations = new AnimationClip[5];
    [SerializeField] private AnimationClip chargedAttackAnimation;

    public float AttackDuration { get { return attackDuration; } }
    public float BlockDuration { get { return blockDuration; } }
    public float SwiftDashDuration { get { return swiftDashDuration; } }
    public float ChargedAttackWindUpDuration { get { return chargedAttackWindUpDuration; } }
    public float AttackDamage { get { return attackDamage; } }
    public float ChargedAttackDamage { get { return chargedAttackDamage; } }
    public float SwiftDashDamage { get { return swiftDashDamage; } }
    public float ParryTimeWindow { get { return parryTimeWindow; } }
    public int MaxComboCount { get { return comboAttackAnimations.Length; } }
    public float ComboResetTime { get { return comboResetTime; } }
    public float ComboInputWindow { get { return comboInputWindow; } }
    public float ComboAttackDuration { get { return comboAttackDuration; } }
    public AnimationClip[] ComboAttackAnimations { get { return comboAttackAnimations; } }
    public AnimationClip ChargedAttackAnimation { get { return chargedAttackAnimation; } }
}
