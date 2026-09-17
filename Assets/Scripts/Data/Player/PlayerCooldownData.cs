using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New PlayerCooldownData", menuName = "Player/CooldownData")]
public class PlayerCooldownData : ScriptableObject
{
    [SerializeField] private string cooldownKey;
    [SerializeField] private float cooldownDuration;
    [SerializeField] private float lastUsedTime;

    public string CooldownKey { get => cooldownKey; }
    public float CooldownDuration { get => cooldownDuration; }
    public float LastUsedTime { get => lastUsedTime; set => lastUsedTime = value; }

    void OnEnable()
    {
        lastUsedTime = -cooldownDuration; // Initialize to allow immediate use
    }

    void OnValidate()
    {
        if (cooldownDuration < 0)
        {
            cooldownDuration = 0;
            Debug.LogWarning("Cooldown duration cannot be negative. Resetting to 0.");
        }
    }

    public bool IsOnCooldown(out float remainingTime)
    {
        remainingTime = Mathf.Max(0, lastUsedTime + cooldownDuration - Time.time);
        return remainingTime > 0;
    }

    public void Use()
    {
        lastUsedTime = Time.time;
        Debug.Log($"Cooldown '{cooldownKey}' used at time {lastUsedTime}. Next available at {lastUsedTime + cooldownDuration}.");
    }
}