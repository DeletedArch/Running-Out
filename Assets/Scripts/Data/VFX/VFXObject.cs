using System;
using UnityEngine;

public class VFXObject : ScriptableObject
{
    [SerializeField] private string vfxName;
    [SerializeField] private GameObject vfxPrefab;
    [SerializeField] private float duration = 1f;

    public event Action<string> OnVFXEnded;
    public void TriggerVFXEnded(string vfxName)
    {
        OnVFXEnded?.Invoke(vfxName);
    }

    public string VFXName => vfxName;
    public GameObject VFXPrefab => vfxPrefab;
    public float Duration => duration;
}