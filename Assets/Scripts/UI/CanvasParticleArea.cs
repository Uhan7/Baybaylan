using System.ComponentModel;
using Coffee.UIExtensions;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform), typeof(UIParticle))]
public class CanvasParticleArea : MonoBehaviour
{
    [SerializeField] private ParticleSystem emitter;
    [SerializeField] private float m_hanginHabagatSimulationSpeed = 1.0f;
    private LevelConfig m_levelConfig;

    private void OnEnable() => Initialize();

    private void Start() => Initialize();

    private void OnRectTransformDimensionsChange() => ResizeEmitter();
    private void Initialize()
    {
        ResizeEmitter();
        GetLevelConfig();
        ApplyHanginHabagatEffect();
    }

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

    private void GetLevelConfig()
    {
        GameManager gameManager = GameManager.Instance;
        if (null == gameManager) return;
        m_levelConfig = gameManager.config;
    }

    private void ApplyHanginHabagatEffect()
    {
        if (null == m_levelConfig) return;
        if(!m_levelConfig.HasPaghihigpit(PaghihigpitTypes.HanginHabagat)) return;
        SetPlaybackSpeed(m_hanginHabagatSimulationSpeed);
    }

    public void SetPlaybackSpeed(float _speed)
    {
        if (emitter == null) return;
        var mainModule = emitter.main;
        mainModule.simulationSpeed = _speed;
    }
}
