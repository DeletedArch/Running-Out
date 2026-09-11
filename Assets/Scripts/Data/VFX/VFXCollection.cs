using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VFX Collection", menuName = "VFX/VFX Collection")]
public class VFXCollection : ScriptableObject
{
    [SerializeField] private VFXObject[] allVFX;
    public VFXObject[] AllVFX => allVFX;
    private Dictionary<string, VFXObject> lookup;

    private void BuildLookup()
    {
        lookup = new Dictionary<string, VFXObject>();
        foreach (var vfx in allVFX)
        {
            if (vfx == null) continue;
            lookup[vfx.VFXName] = vfx;
        }
    }

    public VFXObject GetByName(string name)
    {
        if (lookup == null)
            BuildLookup();

        return lookup.TryGetValue(name, out var vfx) ? vfx : null;
    }

    private void OnEnable()
    {
        lookup = null; // rebuild lazily on next access
    }
}