using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class CooldownUISystem : MonoBehaviour
{
    [SerializeField] private string[] cooldownKeys;
    [SerializeField] private Image[] cooldownFillImages;
    private Dictionary<string, Image> cooldownDictionary;
    void Awake()
    {
        cooldownDictionary = new Dictionary<string, Image>();
        for (int i = 0; i < cooldownFillImages.Length; i++)
        {
            if (cooldownFillImages[i] != null && !string.IsNullOrEmpty(cooldownKeys[i]))
            {
                cooldownDictionary.Add(cooldownKeys[i], cooldownFillImages[i]);
            }
        }
    }

    void OnEnable()
    {
        CooldownSystem.UpdateCooldownUI += UpdateCooldownUI;
    }

    void OnDisable()
    {
        CooldownSystem.UpdateCooldownUI -= UpdateCooldownUI;
    }

    void UpdateCooldownUI(string key, float remainingTime, float totalDuration)
    {
        if (cooldownDictionary.TryGetValue(key, out Image cooldownFillImage))
        {
            float fillAmount = Mathf.Clamp01(remainingTime / totalDuration);
            if (cooldownFillImage != null)
            {
                cooldownFillImage.fillAmount = fillAmount;
            }
        }
    }
}