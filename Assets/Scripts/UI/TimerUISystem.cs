using UnityEngine;
using System;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

public class TimerUISystem : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image fillImageDelayed;
    [SerializeField] private Material[] fillMaterials;
    private CancellationTokenSource stateCts;
    private bool canDepleteDelay = true;
    
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
        TimerSystem.OnTimerChange -= HandleTimerChange;
    }

    void OnEnable()
    {
        TimerSystem.OnTimerUpdated += UpdateUI;
        TimerSystem.OnTimerChange += HandleTimerChange;
    }

    void OnDisable()
    {
        TimerSystem.OnTimerUpdated -= UpdateUI;
        TimerSystem.OnTimerChange -= HandleTimerChange;
    }
    public void UpdateUI(float normalizedValue)
    {
        fillImage.fillAmount = normalizedValue;
        if (canDepleteDelay)
            fillImageDelayed.fillAmount = normalizedValue;
    }

    public void HandleTimerChange(TimerAction action, float changeAmount)
    {
        stateCts?.Cancel();
        stateCts?.Dispose();
        stateCts = new CancellationTokenSource();
        fillImageDelayed.material = fillMaterials[(int)action];
        canDepleteDelay = false;
        UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: stateCts.Token).ContinueWith(() =>
        {
            canDepleteDelay = true;
            fillImageDelayed.fillAmount = fillImage.fillAmount;
        }).Forget();
    }
}

public enum TimerAction
{
    Replenish = 0,
    DepleteAttack = 1,
    DepleteBlock = 2,
    DepleteHit = 3,
}