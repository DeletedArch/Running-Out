using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TargetData
{
    public GameObject Object;
    public float distance;
    [Range(-1f, 1f)]
    public float directionDifference; // 1 = directly in front, 0 = perpendicular, -1 = behind
    public Vector2 directionToObject;

    public TargetData(GameObject Object)
    {
        this.Object = Object;
    }
}

[CreateAssetMenu(fileName = "NewTargetDetectionChannel", menuName = "Detection/Target Detection Channel")]
public class TargetDetectionChannel : ScriptableObject
{
    [SerializeField] private List<TargetData> targets = new List<TargetData>();
    public IReadOnlyList<TargetData> Targets => targets;

    public event Action OnTargetsChanged;

    private void OnEnable()
    {
        targets.Clear();
    }

    public void Add(GameObject Object)
    {
        if (Object == null) return;
        if (targets.Exists(t => t.Object == Object)) return;

        targets.Add(new TargetData(Object));
        OnTargetsChanged?.Invoke();
    }

    public void Remove(GameObject Object)
    {
        if (Object == null) return;

        int removed = targets.RemoveAll(t => t.Object == Object || t.Object == null);
        if (removed > 0)
        {
            OnTargetsChanged?.Invoke();
        }
    }

    public void UpdateTargets(Vector2 origin, Vector2 direction)
    {
        // Purge dead/destroyed objects
        targets.RemoveAll(t => t.Object == null);

        foreach (var target in targets)
        {
            Vector2 toObject = (Vector2)target.Object.transform.position - origin;
            target.distance = toObject.magnitude;
            target.directionToObject = target.distance > 0.001f ? toObject / target.distance : direction;
            target.directionDifference = Vector2.Dot(target.directionToObject, direction);
        }

        targets.Sort((a, b) =>
        {
            bool aInFront = a.directionDifference > 0f;
            bool bInFront = b.directionDifference > 0f;

            if (aInFront && !bInFront) return -1;
            if (!aInFront && bInFront) return 1;

            return a.distance.CompareTo(b.distance);
        });
    }

    public TargetData GetBestTarget(float minDirectionDiff = 0f, float maxDistance = float.MaxValue)
    {
        targets.RemoveAll(t => t.Object == null);

        foreach (var target in targets)
        {
            if (target.directionDifference >= minDirectionDiff && target.distance <= maxDistance)
            {
                return target;
            }
        }

        return null;
    }

    public void Clear()
    {
        targets.Clear();
        OnTargetsChanged?.Invoke();
    }
}
