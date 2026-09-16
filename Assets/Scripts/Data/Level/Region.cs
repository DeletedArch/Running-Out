using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Region", menuName = "LevelGeneration/Region")]
public class Region : ScriptableObject
{
    [SerializeField] private string regionName;
    [SerializeField] private int regionID;
    [SerializeField] private AreaLevel regionLevel;
    [SerializeField] private Area[] availableAreas;
    [SerializeField, Tooltip("The starting point of the region, a new region means a new environment")] private int regionStart;
    [SerializeField] private int regionEnd;

    public string RegionName { get => regionName; }
    public int RegionID { get => regionID; }
    public AreaLevel RegionLevel { get => regionLevel; }
    public Area[] AvailableAreas { get => availableAreas; }

    void OnEnable()
    {
        regionID = UnityEngine.Random.Range(0, 1000000);
    }

    public Area GetRandomArea(int costLevel, out int remainingCost)
    {
        remainingCost = costLevel;
        if (availableAreas.Length > 0)
        {
            Area[] filteredAreas = GetAreasByCostLevel(costLevel);
            if (filteredAreas.Length > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, filteredAreas.Length);
                remainingCost -= filteredAreas[randomIndex].EnemyCount;
                return filteredAreas[randomIndex];
            }
            return null;
        }
        else
        {
            return null;
        }
    }

    Area[] GetAreasByCostLevel(int costLevel)
    {
        return Array.FindAll(availableAreas, area => area.EnemyCount <= costLevel);
    }
}