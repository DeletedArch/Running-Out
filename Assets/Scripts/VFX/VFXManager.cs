using UnityEngine;
using System;

public class VFXManager : MonoBehaviour
{
    [SerializeField] private ScriptableObject[] VFXObjects;

    private void OnEnable()
    {
        VFXEvents.OnVFXTriggered += VFXTriggered;
        VFXEvents.OnVFXPosTriggered += VFXPosTriggered;
    }
    void VFXTriggered(string vfxName)
    {
        
    }

    void VFXPosTriggered(string vfxName, Vector2 position)
    {
        
    }
}