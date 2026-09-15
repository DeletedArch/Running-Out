using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New VFX Object", menuName = "VFX/VFX Object")]
public class VFXObject : ScriptableObject
{
    [SerializeField] private string vfxName;
    [SerializeField] private GameObject vfxPrefab;
    [SerializeField] private float duration = 1f;
    [SerializeField] private Vector2 offset = Vector2.zero;

    public string VFXName => vfxName;
    public GameObject VFXPrefab => vfxPrefab;
    public float Duration => duration;
    public Vector2 Offset => offset;

    void OnValidate()
    {
        
    }
}