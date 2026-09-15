using System;
using UnityEngine;

public static class VFXEvents
{
    public static event Action<string, Vector2, Quaternion, bool, Transform> OnPlayVFX;
    public static event Action<string> OnVFXEnded;

    public static void TriggerVFX(string vfxName, Vector2 position, Quaternion rotation, bool flipX = false, Transform follow = null)
    {
        OnPlayVFX?.Invoke(vfxName, position, rotation, flipX, follow);
    }


    public static void EndVFX(string vfxName)
    {
        OnVFXEnded?.Invoke(vfxName);
    }
}