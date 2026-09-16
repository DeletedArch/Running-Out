using UnityEngine;
using System;
using Random = UnityEngine.Random;

public enum AreaLevel
{
    RegionSakuraTree = 0,
    RegionDeadForest = 1,
    RegionMountain = 2,
    RegionDarkCity = 3,
    Endless = 9999,
}
[CreateAssetMenu(fileName = "New Area", menuName = "LevelGeneration/Area")]
public class Area : ScriptableObject
{
    [SerializeField] private string areaName;
    [SerializeField] private int areaID;
    [Tooltip("Used to determine progression and region")]
    [SerializeField] private AreaLevel areaLevel;
    [SerializeField] private int enemyCount;
    [SerializeField] private GameObject areaPrefab;

    public string AreaName { get => areaName; }
    public int AreaID { get => areaID; }
    public AreaLevel AreaLevel { get => areaLevel; }
    public int EnemyCount { get => enemyCount; }
    public GameObject AreaPrefab { get => areaPrefab; }

    void OnEnable()
    {
        areaID = Random.Range(0, 1000000);
    }
}