using UnityEngine;
using System;
using UnityEngine.UI;

[Serializable]
public class TimerSystem
{
    private Animator animator;
    [SerializeField] private float timer = 20f;
    [SerializeField] private float normalizationRatio = 1f/20f;
    [SerializeField] private float maxTime = 35f;
    [SerializeField] private float minTime = 0f;
    [SerializeField] private float highSoftCap = 30f;
    [SerializeField] private float lowSoftCap = 5f;
    [SerializeField] private float depletionRate = 1f;

    public float Timer => timer;
    public float NormalizedTimer => Math.Clamp(timer, lowSoftCap, highSoftCap) * normalizationRatio;

    public static event Action<float> OnTimerUpdated;
    public static event Action OnTimerDepleted;

    public TimerSystem(Animator animator, float initialTime = 20f)
    {
        this.animator = animator;
        timer = initialTime;
        ITimerAccess.OnTimerChange += HandleTimerChange;
    }

    public void Update(float deltaTime)
    {
        DepleteTimer(depletionRate * deltaTime);
        timer = Mathf.Clamp(timer, minTime, maxTime);
        animator.SetFloat("Timer", NormalizedTimer);
        if (timer <= 0)
        {
            OnTimerDepleted?.Invoke();
        }
        OnTimerUpdated?.Invoke(timer/maxTime);
        // timerUI.UpdateUI(timer/maxTime);
    }

    private void DepleteTimer(float amount)
    {
        timer -= amount;
    }

    public void ReplenishTimer(float amount)
    {
        timer += amount;
    }

    private void HandleTimerChange(float changeAmount)
    {
        Debug.Log($"Timer change event received: {changeAmount}");
        ReplenishTimer(changeAmount);
    }
}

