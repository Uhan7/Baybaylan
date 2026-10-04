using System;
using System.Collections.Generic;
using UnityEngine;

public class MouseParallax : MonoBehaviour
{
    [Serializable]
    private class ParallaxLayer
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private Vector2 movementAmount = new Vector2(10f, 10f);

        private Vector2 restingPosition;

        public void CacheRestingPosition()
        {
            if (target != null)
                restingPosition = target.anchoredPosition;
        }

        public void Move(Vector2 mouseOffset, float direction, float smoothingAmount)
        {
            if (target == null)
                return;

            Vector2 targetPosition = restingPosition
                + Vector2.Scale(mouseOffset, movementAmount) * direction;

            target.anchoredPosition = Vector2.Lerp(
                target.anchoredPosition,
                targetPosition,
                smoothingAmount);
        }

        public void RestoreRestingPosition()
        {
            if (target != null)
                target.anchoredPosition = restingPosition;
        }
    }

    [Header("Layers")]
    [SerializeField] private List<ParallaxLayer> layers = new List<ParallaxLayer>();

    [Header("Movement")]
    [SerializeField, Min(0f)] private float smoothing = 8f;
    [SerializeField] private bool moveOppositeCursor = true;
    [SerializeField] private bool useUnscaledTime = true;

    private void OnEnable()
    {
        foreach (ParallaxLayer layer in layers)
            layer?.CacheRestingPosition();
    }

    private void LateUpdate()
    {
        Vector2 mouseOffset = GetNormalizedMouseOffset();
        float direction = moveOppositeCursor ? -1f : 1f;
        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float smoothingAmount = smoothing <= 0f
            ? 1f
            : 1f - Mathf.Exp(-smoothing * deltaTime);

        foreach (ParallaxLayer layer in layers)
            layer?.Move(mouseOffset, direction, smoothingAmount);
    }

    private void OnDisable()
    {
        foreach (ParallaxLayer layer in layers)
            layer?.RestoreRestingPosition();
    }

    private static Vector2 GetNormalizedMouseOffset()
    {
        float width = Mathf.Max(1f, Screen.width);
        float height = Mathf.Max(1f, Screen.height);

        return new Vector2(
            Mathf.Clamp(Input.mousePosition.x / width * 2f - 1f, -1f, 1f),
            Mathf.Clamp(Input.mousePosition.y / height * 2f - 1f, -1f, 1f));
    }
}
