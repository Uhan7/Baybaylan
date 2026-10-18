using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmoothGridLayoutGroup : GridLayoutGroup
{
    [Header("Smooth Movement")]
    [SerializeField, Min(0f)] private float moveSmoothTime = 0.09f;
    [SerializeField, Min(0f)] private float maxMoveSpeed = 3000f;

    private readonly Dictionary<RectTransform, Vector2> currentPositions =
        new Dictionary<RectTransform, Vector2>();
    private readonly Dictionary<RectTransform, Vector2> targetPositions =
        new Dictionary<RectTransform, Vector2>();
    private readonly Dictionary<RectTransform, Vector2> velocities =
        new Dictionary<RectTransform, Vector2>();
    private readonly List<RectTransform> staleChildren = new List<RectTransform>();

    public override void SetLayoutVertical()
    {
        base.SetLayoutVertical();
        if (!Application.isPlaying) return;

        RemoveStaleChildren();

        for (int i = 0; i < rectChildren.Count; i++)
        {
            RectTransform child = rectChildren[i];
            Vector2 target = child.anchoredPosition;
            targetPositions[child] = target;

            if (!currentPositions.TryGetValue(child, out Vector2 current))
            {
                current = target;
                currentPositions[child] = current;
                velocities[child] = Vector2.zero;
            }

            child.anchoredPosition = current;
        }
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying) return;

        float safeSmoothTime = Mathf.Max(0.0001f, moveSmoothTime);
        float safeMaxSpeed = maxMoveSpeed <= 0f ? Mathf.Infinity : maxMoveSpeed;

        foreach (KeyValuePair<RectTransform, Vector2> entry in targetPositions)
        {
            RectTransform child = entry.Key;
            if (child == null || child.parent != transform) continue;

            Vector2 current = currentPositions[child];
            Vector2 velocity = velocities[child];
            Vector2 next = moveSmoothTime <= 0f
                ? entry.Value
                : Vector2.SmoothDamp(
                    current,
                    entry.Value,
                    ref velocity,
                    safeSmoothTime,
                    safeMaxSpeed,
                    Time.deltaTime);

            currentPositions[child] = next;
            velocities[child] = velocity;
            child.anchoredPosition = next;
        }
    }

    public void SetCurrentPosition(RectTransform child, Vector2 anchoredPosition)
    {
        if (child == null) return;

        currentPositions[child] = anchoredPosition;
        velocities[child] = Vector2.zero;
    }

    private void RemoveStaleChildren()
    {
        staleChildren.Clear();

        foreach (RectTransform child in currentPositions.Keys)
        {
            if (child == null || child.parent != transform)
                staleChildren.Add(child);
        }

        foreach (RectTransform child in staleChildren)
        {
            currentPositions.Remove(child);
            targetPositions.Remove(child);
            velocities.Remove(child);
        }
    }
}
