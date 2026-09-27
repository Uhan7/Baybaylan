using Coffee.UIExtensions;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform), typeof(UIParticle))]
public class CanvasParticleArea : MonoBehaviour
{
    [SerializeField] private ParticleSystem emitter;

    private void OnEnable() => ResizeEmitter();

    private void Start() => ResizeEmitter();

    private void OnRectTransformDimensionsChange() => ResizeEmitter();

    private void ResizeEmitter()
    {
        if (emitter == null)
            return;

        var rect = GetComponent<RectTransform>().rect;
        var scale = GetComponent<UIParticle>().scale;
        if (rect.width <= 0 || rect.height <= 0 || scale <= 0)
            return;

        var shape = emitter.shape;
        shape.shapeType = ParticleSystemShapeType.Rectangle;
        shape.scale = new Vector3(rect.width / scale, rect.height / scale, 1f);
    }
}
