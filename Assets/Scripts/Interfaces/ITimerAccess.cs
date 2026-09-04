using UnityEngine;
using System;

public interface ITimerAccess
{
    public float TimerUsage { get; }
    public float TimerRestoration { get; }
    public static event Action<float> OnTimerChange;

    public static void ModifyTimer(float amount)
    {
        OnTimerChange?.Invoke(amount);
    }
}