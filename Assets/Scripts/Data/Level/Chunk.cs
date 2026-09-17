using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Chunk
{
    [SerializeField] private List<Area> areas = new List<Area>();
    [SerializeField] private List<GameObject> spawnedInstances = new List<GameObject>();

    public List<Area> Areas { get => areas; }
    public float StartX { get; set; }
    public float EndX { get; set; }

    public Area GetLastArea()
    {
        if (areas.Count > 0)
        {
            return areas[areas.Count - 1];
        }
        else
        {
            return null;
        }
    }

    public Area GetFirstArea()
    {
        if (areas.Count > 0)
        {
            return areas[0];
        }
        else
        {
            return null;
        }
    }

    public void AddSpawnedInstance(GameObject instance)
    {
        spawnedInstances.Add(instance);
    }

    public void UnloadArea(int index)
    {
        if (index >= 0 && index < areas.Count)
        {
            areas.RemoveAt(index);
        }
        if (index >= 0 && index < spawnedInstances.Count)
        {
            if (spawnedInstances[index] != null)
            {
                UnityEngine.Object.Destroy(spawnedInstances[index]);
            }
            spawnedInstances.RemoveAt(index);
        }
    }

    public void UnloadAllAreas()
    {
        foreach (var instance in spawnedInstances)
        {
            if (instance != null)
            {
                UnityEngine.Object.Destroy(instance);
            }
        }
        spawnedInstances.Clear();
        areas.Clear();
    }
}