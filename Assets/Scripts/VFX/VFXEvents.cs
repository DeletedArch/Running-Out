using System;
using UnityEngine;
public static class VFXEvents
{
    public static event Action<string> OnVFXTriggered;
    public static event Action<string, Vector2> OnVFXPosTriggered;

    public static void TriggerVFX(string vfxName)
    {
        OnVFXTriggered?.Invoke(vfxName);
    }

    public static void TriggerVFX(string vfxName, Vector2 position)
    {
        OnVFXPosTriggered?.Invoke(vfxName, position);
    }
}