using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class CooldownSystem
{
    [SerializeField] private PlayerCooldownData[] cooldowns;
    
    private Dictionary<string, PlayerCooldownData> cooldownDictionary;

    public static event Action<string, float, float> UpdateCooldownUI;

    public void Initialize()
    {
        cooldownDictionary = new Dictionary<string, PlayerCooldownData>();
        foreach (var cooldown in cooldowns)
        {
            if (!cooldownDictionary.ContainsKey(cooldown.CooldownKey))
            {
                cooldownDictionary.Add(cooldown.CooldownKey, cooldown);
            }
            else
            {
                Debug.LogWarning($"Duplicate cooldown key found: {cooldown.CooldownKey}. Only the first instance will be used.");
            }
        }
    }

    public bool IsOnCooldown(string key)
    {
        if (cooldownDictionary.TryGetValue(key, out PlayerCooldownData cooldownData))
        {
            return cooldownData.IsOnCooldown(out _);
        }
        else
        {
            Debug.LogWarning($"Cooldown key not found: {key}");
            return false;
        }
    }

    public bool UseCooldown(string key)
    {
        if (cooldownDictionary.TryGetValue(key, out PlayerCooldownData cooldownData))
        {
            if (!cooldownData.IsOnCooldown(out _))
            {
                cooldownData.Use();
                return true;
            }
            else
            {
                return false; // Still on cooldown
            }
        }
        else
        {
            Debug.LogWarning($"Cooldown key not found: {key}");
            return false;
        }
    }

    public void UpdateCooldowns()
    {
        foreach (var cooldown in cooldownDictionary.Values)
        {
            if (cooldown.IsOnCooldown(out float remainingTime))
            {
                UpdateCooldownUI?.Invoke(cooldown.CooldownKey, remainingTime, cooldown.CooldownDuration);
            }
        }
    }
}