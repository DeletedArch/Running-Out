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
    [SerializeField] private int areaID = Random.Range(0, 1000000);
    [Tooltip("Used to determine progression and region")]
    [SerializeField] private AreaLevel areaLevel;
    [SerializeField] private int enemyCount;
    [SerializeField] private GameObject areaPrefab;
    [SerializeField] private Transform startingPoint;
    [SerializeField] private Transform exitPoint;

    public string AreaName { get => areaName; }
    public int AreaID { get => areaID; }
    public AreaLevel AreaLevel { get => areaLevel; }
    public int EnemyCount { get => enemyCount; }
    public GameObject AreaPrefab { get => areaPrefab; }
    public Transform StartingPoint { get => startingPoint; }
    public Transform ExitPoint { get => exitPoint; }
    [Header("Runtime")]
    private GameObject areaInstance;
    private bool isAreaInstantiated = false;
    
    public void InistantiateArea(Vector2 position)
    {
        if (areaPrefab != null && !isAreaInstantiated)
        {
            areaInstance = Instantiate(areaPrefab);
            areaInstance.name = areaName;
            areaInstance.transform.position = position; // Set the position to the origin or any desired position
            isAreaInstantiated = true;
        }
        else
        {
            if (isAreaInstantiated)
            {
                Debug.LogWarning("Area is already instantiated: " + areaName);
            }
            else
            {
                Debug.LogWarning("Area prefab is not assigned for " + areaName);
            }
        }
    }

    public void DestroyArea()
    {
        Destroy(areaInstance);
        Destroy(this);
    }
}