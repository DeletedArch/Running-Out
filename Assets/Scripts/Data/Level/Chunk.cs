using UnityEngine;
using System;
using System.Collections.Generic;

// This is a runtime object that generates multiple areas and stores them
// Chunks are meant to ease generation stutters by generating multiple areas at once and storing them in a single object

public class Chunk : ScriptableObject
{
    [SerializeField] private List<Area> areas = new List<Area>();

    public List<Area> Areas { get => areas; }

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

    public void UnloadArea(int index)
    {
        if (index >= 0 && index < areas.Count)
        {
            Area area = areas[index];
            Destroy(area);
            areas.RemoveAt(index);
        }
    }
}