using UnityEngine;
using System;
using UnityEngine.UI;

[Serializable]
public class TimerSystem
{
    [SerializeField] private Animator animator;
    [SerializeField] private float timer = 20f;
    [SerializeField] private float normalizationRatio = 1f/20f;
    [SerializeField] private float maxTime = 35f;
    [SerializeField] private float minTime = 0f;
    [SerializeField] private float highSoftCap = 30f;
    [SerializeField] private float lowSoftCap = 5f;
    [SerializeField] private float depletionRate = 1f;
    [SerializeField] private bool debugDoNotDeplete = true;

    public float Timer => timer;
    public float NormalizedTimer => Math.Clamp(timer, lowSoftCap, highSoftCap) * normalizationRatio;

    public static event Action<float> OnTimerUpdated;
    public static event Action OnTimerDepleted;
    public static event Action<TimerAction, float> OnTimerChange;

    public TimerSystem()
    {
        ITimerAccess.OnTimerChange += HandleTimerChange;
    }

    public void Update(float deltaTime)
    {
        if (debugDoNotDeplete) return;
        LoopDepleteTimer(depletionRate * deltaTime);
        timer = Mathf.Clamp(timer, minTime, maxTime);
        animator.SetFloat("Timer", NormalizedTimer);
        if (timer <= 0)
        {
            OnTimerDepleted?.Invoke();
        }
        OnTimerUpdated?.Invoke(timer/maxTime);
        // timerUI.UpdateUI(timer/maxTime);
    }

    void LoopDepleteTimer(float amount)
    {
        if (debugDoNotDeplete) return;
        timer -= amount;
    }

    public void DepleteTimer(float amount, TimerAction action = TimerAction.DepleteAttack)
    {
        if (debugDoNotDeplete) return;
        timer -= amount;
        OnTimerChange?.Invoke(action, amount);
    }

    public void ReplenishTimer(float amount, TimerAction action = TimerAction.Replenish)
    {
        if (debugDoNotDeplete) return;
        timer += amount;
        OnTimerChange?.Invoke(action, amount);
    }

    private void HandleTimerChange(float changeAmount)
    {
        if (debugDoNotDeplete) return;
        Debug.Log($"Timer change event received: {changeAmount}");
        if (changeAmount < 0)
        {
            DepleteTimer(-changeAmount, TimerAction.DepleteAttack);
        }
        else
        {
            ReplenishTimer(changeAmount);
        }
    }
}

