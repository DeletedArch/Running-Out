using UnityEngine;
using System;
using UnityEngine.UI;

public class TimerUISystem : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Image fillImage;

    public TimerUISystem(GameObject panel, Image fillImage)
    {
        this.panel = panel;
        this.fillImage = fillImage;
    }
    
    void Awake()
    {
        if (panel == null)
        {
            Debug.LogError("Panel is not assigned in TimerUISystem.");
        }
        if (fillImage == null)
        {
            Debug.LogError("Fill Image is not assigned in TimerUISystem.");
        }
    }

    void OnDestroy()
    {
        TimerSystem.OnTimerUpdated -= UpdateUI;
    }

    void OnEnable()
    {
        TimerSystem.OnTimerUpdated += UpdateUI;
    }

    void OnDisable()
    {
        TimerSystem.OnTimerUpdated -= UpdateUI;
    }
    public void UpdateUI(float normalizedValue)
    {
        fillImage.fillAmount = normalizedValue;
    }
}