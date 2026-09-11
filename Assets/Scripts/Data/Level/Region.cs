using UnityEngine;
using System;


public class Region : ScriptableObject
{
    [SerializeField] private string regionName;
    [SerializeField] private int regionID = UnityEngine.Random.Range(0, 1000000);
    [SerializeField] private AreaLevel regionLevel;
    [SerializeField] private Area[] availableAreas;
    [SerializeField, Tooltip("The starting point of the region, a new region means a new environment")] private int regionStart;
    [SerializeField] private int regionEnd;

    public string RegionName { get => regionName; }
    public int RegionID { get => regionID; }
    public AreaLevel RegionLevel { get => regionLevel; }
    public Area[] AvailableAreas { get => availableAreas; }

    public Area GetRandomArea()
    {
        if (availableAreas.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableAreas.Length);
            return availableAreas[randomIndex];
        }
        else
        {
            return null;
        }
    }
}