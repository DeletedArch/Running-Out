using UnityEngine;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public static class ActionHelpers
{
    static List<Animator> activeAnimators = new List<Animator>();
    public static UniTask ApplyHitstop(Animator[] animators, float hitstopDuration)
    {
        if (animators == null || animators.Length == 0) return UniTask.CompletedTask;

        foreach (Animator animator in animators)
        {
            if (animator == null) continue;

            if (!activeAnimators.Contains(animator))
            {
                activeAnimators.Add(animator);
                animator.speed = 0.1f;
            }
        }

        Debug.Log($"ActionHelpers: Applying hitstop for {hitstopDuration} seconds to {animators.Length} animators.");

        return UniTask.Delay(TimeSpan.FromSeconds(hitstopDuration)).ContinueWith(() =>
        {
            foreach (Animator animator in animators)
            {
                if (animator == null) continue;

                if (activeAnimators.Contains(animator))
                {
                    activeAnimators.Remove(animator);
                    animator.speed = 1f;
                }
            }
        });
    }

    public async static UniTask ApplyGlobalHitstop(float hitstopDuration, float hitstopRestorationTime = 0f, float timeScaleDuringHitstop = 0.1f)
    {
        if (hitstopDuration <= 0f) return;
        Time.timeScale = timeScaleDuringHitstop;
        await UniTask.Delay(TimeSpan.FromSeconds(hitstopDuration), DelayType.UnscaledDeltaTime);

        if (hitstopRestorationTime > 0f)
        {
            float elapsedTime = 0f;
            while (elapsedTime < hitstopRestorationTime)
            {
                Time.timeScale = Mathf.Lerp(timeScaleDuringHitstop, 1f, elapsedTime / hitstopRestorationTime);
                elapsedTime += Time.unscaledDeltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update);
            }
            Time.timeScale = 1f;
        }
        Time.timeScale = 1f;
    }
}