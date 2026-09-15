using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Cysharp.Threading.Tasks;

public class VFXManager : MonoBehaviour
{
    [SerializeField] private VFXCollection vfxCollection;
    private readonly Dictionary<string, ObjectPool<GameObject>> _pools = new();
    private List<(GameObject, Transform, VFXObject)> followerVFX = new List<(GameObject, Transform, VFXObject)>();

    private void Awake()
    {
        if (vfxCollection == null || vfxCollection.AllVFX == null) return;

        foreach (var vfx in vfxCollection.AllVFX)
        {
            if (vfx == null || vfx.VFXPrefab == null) continue;

            var prefab = vfx.VFXPrefab;
            _pools[vfx.VFXName] = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(prefab),
                actionOnGet: obj => obj.SetActive(true),
                actionOnRelease: obj => obj.SetActive(false),
                actionOnDestroy: Destroy
            );
        }
    }

    private void OnEnable() => VFXEvents.OnPlayVFX += PlayVFX;
    private void OnDisable() => VFXEvents.OnPlayVFX -= PlayVFX;

    private void PlayVFX(string vfxName, Vector2 position, Quaternion rotation, bool flipX, Transform follow)
    {
        Debug.Log($"PlayVFX called with name: {vfxName}, position: {position}, rotation: {rotation}, flipX: {flipX}, follow: {follow}");
        if (!_pools.TryGetValue(vfxName, out var pool)) {
            Debug.LogError($"No VFX pool found for name: {vfxName}");
            return;
        };

        var vfx = pool.Get();
        if (vfx == null) {
            Debug.LogError($"Failed to get VFX instance from pool for name: {vfxName}");
            return;
        }
        var vfxData = vfxCollection.GetByName(vfxName);

        vfx.transform.rotation = rotation;

        vfx.transform.position = position + (vfxData != null
            ? (vfx.transform.right * vfxData.Offset.x) + (vfx.transform.up * vfxData.Offset.y) // Offset is rotation aware
            : Vector2.zero);


        var sr = vfx.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.flipX = flipX;
        }
        if (follow != null)
        {
            followerVFX.Add((vfx, follow, vfxData));
        }

        float duration = vfxData != null ? vfxData.Duration : 1f;
        ReleaseAfterDelay(vfx, duration, pool).Forget();
    }

    private async UniTaskVoid ReleaseAfterDelay(GameObject vfx, float delay, ObjectPool<GameObject> pool)
    {
        Animator animator = vfx.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play("VFX", 0, 0f);
        }
        await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: vfx.GetCancellationTokenOnDestroy());
        pool.Release(vfx);
    }

    void Update()
    {
        for (int i = followerVFX.Count - 1; i >= 0; i--)
        {
            var vfx = followerVFX[i];
            if (vfx.Item1 == null || vfx.Item1.activeSelf == false)
            {
                followerVFX.RemoveAt(i);
                continue;
            }

            var followTransform = vfx.Item2;
            if (followTransform != null)
            {
                vfx.Item1.transform.position = followTransform.position + (vfx.Item3 != null
            ? (vfx.Item2.transform.right * vfx.Item3.Offset.x) + (vfx.Item2.transform.up * vfx.Item3.Offset.y) // Offset is rotation aware
            : Vector2.zero);
            }
        }
    }
}