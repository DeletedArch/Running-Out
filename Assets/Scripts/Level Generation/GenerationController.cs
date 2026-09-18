using UnityEngine;
using System;
using System.Collections.Generic;

public class GenerationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject ground;
    [SerializeField] private Vector2 chunkOffset = new Vector2(50, 0);
    [SerializeField] private Vector2 areaOffset = new Vector2(10, 0);
    [SerializeField] private Vector2 placePoint = new Vector2(0, 0);

    [Header("Level Generation")]
    [SerializeField] private Queue<Chunk> chunkRenderQueue = new Queue<Chunk>();
    [SerializeField] private List<Chunk> activeChunks = new List<Chunk>();
    [SerializeField] private List<Region> regions = new List<Region>();

    [Header("Settings")]
    [SerializeField] private int maxChunks = 3;
    [SerializeField] private int maxAreasPerChunk = 5;
    [SerializeField] private int minAreasPerChunk = 3;
    [SerializeField] private float chunkGenerationDistance = 50f;
    [SerializeField] private float chunkUnloadDistance = 100f;
    [SerializeField] private float safeBufferBehindPlayer = 20f;

    [Header("Region Progression")]
    [SerializeField] private AreaLevel currentAreaLevel = AreaLevel.RegionSakuraTree;
    [SerializeField] private float distanceInCurrentLevel = 0f;
    [SerializeField] private int areaIndex = 0;
    [SerializeField] private int currentSpawningCost = 15;
    [SerializeField] private int costChangeTimes = 0;

    [Header("Region Progression Settings")]
    [SerializeField] private int costBase = 2;
    [SerializeField] private float costMultiplier = 1;
    [SerializeField] private int distancePerCostIncrease = 400;

    void Awake()
    {
        if (playerTransform == null)
        {
            var player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("PlayerController not found in the scene. Please assign the playerTransform manually.");
            }
        }
        if (ground == null)
        {
            Debug.LogError("Ground not found in the scene. Please assign the ground manually.");
        }
        GenerateChunk();
        RenderChunk();
    }

    Region GetRegionForCurrentLevel()
    {
        foreach (var region in regions)
        {
            if (region.RegionLevel == currentAreaLevel)
            {
                return region;
            }
        }
        return null;
    }

    void GenerateChunk()
    {
        var chunk = new Chunk();
        chunkRenderQueue.Enqueue(chunk);
        var region = GetRegionForCurrentLevel();
        if (region != null)
        {
            int remainingCost = currentSpawningCost;
            for (int i = 0; i < UnityEngine.Random.Range(minAreasPerChunk, maxAreasPerChunk + 1); i++)
            {
                var area = region.GetRandomArea(remainingCost, out remainingCost);
                if (area != null)
                {
                    chunk.Areas.Add(area);
                }
            }
        }
        else
        {
            Debug.LogWarning("No region found for the current area level.");
        }
    }

    void RenderChunk()
    {
        if (chunkRenderQueue.Count > 0)
        {
            var chunk = chunkRenderQueue.Dequeue();
            chunk.StartX = placePoint.x;

            foreach (var area in chunk.Areas)
            {
                if (area.AreaPrefab != null)
                {
                    GameObject instance = Instantiate(area.AreaPrefab, placePoint + areaOffset + (area.AreaPrefab.transform.localScale.x * Vector2.right * 0.5f), Quaternion.identity);
                    chunk.AddSpawnedInstance(instance);
                    placePoint = (area.AreaPrefab.transform.localScale.x * Vector2.right * 0.5f) + instance.transform.position.x * Vector2.right;
                }
            }

            chunk.EndX = placePoint.x;
            activeChunks.Add(chunk);
            chunkGenerationDistance = placePoint.x - chunkOffset.x;

            if (activeChunks.Count == 1)
            {
                UpdateUnloadDistance();
            }
        }
    }

    void UnloadChunk()
    {
        if (activeChunks.Count > 0)
        {
            var chunkToUnload = activeChunks[0];
            chunkToUnload.UnloadAllAreas();
            activeChunks.RemoveAt(0);
            UpdateUnloadDistance();
        }
    }

    void UpdateUnloadDistance()
    {
        if (activeChunks.Count > 0)
        {
            chunkUnloadDistance = activeChunks[0].EndX + safeBufferBehindPlayer;
        }
        else
        {
            chunkUnloadDistance = float.MaxValue;
        }
    }

    void Update()
    {
        if (playerTransform != null)
        {
            distanceInCurrentLevel = playerTransform.position.x;
            ground.transform.position = new Vector3(distanceInCurrentLevel, ground.transform.position.y, ground.transform.position.z);
            if (distanceInCurrentLevel >= chunkGenerationDistance)
            {
                GenerateChunk();
                RenderChunk();
            }

            if (distanceInCurrentLevel >= chunkUnloadDistance && activeChunks.Count > 0)
            {
                UnloadChunk();
            }

            if (distanceInCurrentLevel >= distancePerCostIncrease * costChangeTimes)
            {
                currentSpawningCost = Mathf.RoundToInt(currentSpawningCost + costBase * costMultiplier);
                costChangeTimes += Mathf.RoundToInt(1 * costMultiplier);
                costMultiplier += 0.1f;
            }
        }
    }
}