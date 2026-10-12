#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using Coffee.UIExtensions;
using UnityEngine;

public static class WordSubmissionAnimationAssetCreator
{
    private const string RootFolder = "Assets/Animations/Word Submission";
    private const string ControllerPath = RootFolder + "/Word Submission.controller";
    private const string AttackClipPath = RootFolder + "/Attack.anim";
    private const string HealClipPath = RootFolder + "/Heal.anim";
    private const string HitClipPath = RootFolder + "/Hit.anim";
    private const string LeftSidePrefabPath = "Assets/Prefabs/Side UI/Left Side Container Variant.prefab";
    private const string RightSidePrefabPath = "Assets/Prefabs/Side UI/Right Side Container Variant.prefab";
    private const string AttackImpactPrefabPath = "Assets/Prefabs/VFX/Attack Impact Particles.prefab";
    private const string HealAuraPrefabPath = "Assets/Prefabs/VFX/Heal Aura Particles.prefab";
    private const string HealMagicPrefabPath = "Assets/Prefabs/VFX/Heal Magic Particles.prefab";

    [MenuItem("Tools/Baybaylan/Set Up Word Submission Animations")]
    public static void CreateAnimationAssets()
    {
        EnsureFolderExists();

        AnimationClip attackClip = GetOrCreateClip(AttackClipPath, "Attack");
        AnimationClip healClip = GetOrCreateHealClip();
        AnimationClip hitClip = GetOrCreateHitClip();

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        }

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        AnimatorState idleState = EnsureState(stateMachine, "Idle", null);
        EnsureState(stateMachine, "Attack", attackClip);
        EnsureState(stateMachine, "Heal", healClip);
        EnsureState(stateMachine, "Hit", hitClip);
        if (stateMachine.defaultState == null) stateMachine.defaultState = idleState;

        GameObject attackImpactPrefab = GetOrCreateAttackImpactParticlePrefab();
        GameObject healAuraPrefab = GetOrCreateHealParticlePrefab(
            HealAuraPrefabPath,
            "Heal Aura Particles",
            true);
        GameObject healMagicPrefab = GetOrCreateHealParticlePrefab(
            HealMagicPrefabPath,
            "Heal Magic Particles",
            false);

        ConfigureLeftSidePrefab(controller, healMagicPrefab);
        ConfigureRightSidePrefab(controller, attackImpactPrefab, healAuraPrefab);

        AssetDatabase.SaveAssets();

        Debug.Log($"Word submission animations are set up in {RootFolder}.");
    }

    private static AnimationClip GetOrCreateClip(string path, string clipName)
    {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip != null) return clip;

        clip = new AnimationClip
        {
            name = clipName,
            frameRate = 60f,
            wrapMode = WrapMode.Once
        };

        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static AnimationClip GetOrCreateHitClip()
    {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(HitClipPath);
        if (clip != null) return clip;

        clip = GetOrCreateClip(HitClipPath, "Hit");
        ApplyDefaultHitCurves(clip);
        return clip;
    }

    private static AnimationClip GetOrCreateHealClip()
    {
        AnimationClip clip = GetOrCreateClip(HealClipPath, "Heal");
        if (AnimationUtility.GetCurveBindings(clip).Length == 0 &&
            AnimationUtility.GetObjectReferenceCurveBindings(clip).Length == 0)
        {
            ApplyDefaultHealCurves(clip);
        }

        return clip;
    }

    private static AnimatorState EnsureState(AnimatorStateMachine stateMachine, string stateName, Motion motion)
    {
        foreach (ChildAnimatorState childState in stateMachine.states)
        {
            if (childState.state.name != stateName) continue;
            if (childState.state.motion == null && motion != null) childState.state.motion = motion;
            return childState.state;
        }

        AnimatorState state = stateMachine.AddState(stateName);
        state.motion = motion;
        return state;
    }

    private static void EnsureFolderExists()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Animations"))
            AssetDatabase.CreateFolder("Assets", "Animations");

        if (!AssetDatabase.IsValidFolder(RootFolder))
            AssetDatabase.CreateFolder("Assets/Animations", "Word Submission");
    }

    private static void ConfigureLeftSidePrefab(
        AnimatorController controller,
        GameObject healMagicPrefab)
    {
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(LeftSidePrefabPath);

        try
        {
            if (prefabRoot.transform.Find("Portrait Holder") == null)
            {
                Debug.LogWarning($"Could not find Portrait Holder in {LeftSidePrefabPath}.");
                return;
            }

            Animator animator = prefabRoot.GetComponent<Animator>();
            if (animator == null) animator = prefabRoot.AddComponent<Animator>();

            string currentControllerPath = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController);
            if (animator.runtimeAnimatorController == null ||
                currentControllerPath == ControllerPath)
            {
                animator.runtimeAnimatorController = controller;
            }
            else
            {
                Debug.LogWarning(
                    $"Kept the custom Animator Controller already assigned to {prefabRoot.name}. " +
                    $"Add Attack and Heal states to that controller or assign {ControllerPath} manually.");
            }

            WordSubmissionAnimationPlayer player = prefabRoot.GetComponent<WordSubmissionAnimationPlayer>();
            if (player == null) player = prefabRoot.AddComponent<WordSubmissionAnimationPlayer>();

            Transform portraitHolder = prefabRoot.transform.Find("Portrait Holder");
            RemoveChildIfPresent(portraitHolder, "Heal Aura Particles");
            ParticleSystem healMagic = EnsureParticleInstance(portraitHolder, healMagicPrefab);

            SerializedObject serializedPlayer = new SerializedObject(player);
            SerializedProperty animatorProperty = serializedPlayer.FindProperty("targetAnimator");
            animatorProperty.objectReferenceValue = animator;
            serializedPlayer.FindProperty("overrideAttackParticleColor").boolValue = false;
            serializedPlayer.FindProperty("attackParticleColor").colorValue = Color.white;
            serializedPlayer.FindProperty("attackParticleColorB").colorValue = Color.white;
            serializedPlayer.FindProperty("healMagicParticles").objectReferenceValue = healMagic;
            serializedPlayer.FindProperty("healAnimationSpeed").floatValue = 1.75f;
            serializedPlayer.FindProperty("healMagicDelay").floatValue = 23f / 60f;
            serializedPlayer.FindProperty("healEffectsStopDelay").floatValue = 0.58f;
            serializedPlayer.FindProperty("targetHealDelay").floatValue = 0.92f;
            serializedPlayer.FindProperty("healPoseHoldNormalizedTime").floatValue = 0.7f;
            serializedPlayer.FindProperty("healPoseHoldDuration").floatValue = 0.8f;
            serializedPlayer.FindProperty("healReturnDelayAfterTarget").floatValue = 0f;
            serializedPlayer.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, LeftSidePrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    private static void ConfigureRightSidePrefab(
        AnimatorController controller,
        GameObject attackImpactPrefab,
        GameObject healAuraPrefab)
    {
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(RightSidePrefabPath);

        try
        {
            if (prefabRoot.transform.Find("Portrait Holder") == null)
            {
                Debug.LogWarning($"Could not find Portrait Holder in {RightSidePrefabPath}.");
                return;
            }

            Animator animator = prefabRoot.GetComponent<Animator>();
            if (animator == null) animator = prefabRoot.AddComponent<Animator>();

            string currentControllerPath = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController);
            if (animator.runtimeAnimatorController == null || currentControllerPath == ControllerPath)
            {
                animator.runtimeAnimatorController = controller;
            }
            else
            {
                Debug.LogWarning(
                    $"Kept the custom Animator Controller already assigned to {prefabRoot.name}. " +
                    $"Add a Hit state to that controller or assign {ControllerPath} manually.");
            }

            StoryTargetAnimationPlayer player = prefabRoot.GetComponent<StoryTargetAnimationPlayer>();
            if (player == null) player = prefabRoot.AddComponent<StoryTargetAnimationPlayer>();

            Transform portraitHolder = prefabRoot.transform.Find("Portrait Holder");
            Transform characterImage = portraitHolder.Find("Character Image");
            ParticleSystem attackImpact = EnsureParticleInstance(portraitHolder, attackImpactPrefab);
            ParticleSystem healAura = EnsureParticleInstance(portraitHolder, healAuraPrefab);

            SerializedObject serializedPlayer = new SerializedObject(player);
            serializedPlayer.FindProperty("targetAnimator").objectReferenceValue = animator;
            serializedPlayer.FindProperty("attackImpactParticles").objectReferenceValue = attackImpact;
            serializedPlayer.FindProperty("healCharacterGraphic").objectReferenceValue =
                characterImage != null ? characterImage.GetComponent<UnityEngine.UI.Graphic>() : null;
            serializedPlayer.FindProperty("healAuraParticles").objectReferenceValue = healAura;
            serializedPlayer.FindProperty("healAuraEmissionDuration").floatValue = 0.25f;
            serializedPlayer.FindProperty("healDarkenDuration").floatValue = 0.22f;
            serializedPlayer.FindProperty("healDarkHoldDuration").floatValue = 0.18f;
            serializedPlayer.FindProperty("healRestoreDuration").floatValue = 0.4f;
            serializedPlayer.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, RightSidePrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    private static void ApplyDefaultHitCurves(AnimationClip clip)
    {
        float[] times = { 0f, 2f / 60f, 5f / 60f, 9f / 60f, 14f / 60f };

        SetCurve(clip, times, "m_AnchoredPosition.x", -20f, 18f, 30f, -28f, -20f);
        SetCurve(clip, times, "m_AnchoredPosition.y", -20f, -12f, -16f, -20f, -20f);
        SetCurve(clip, times, "localEulerAnglesRaw.x", 0f, 0f, 0f, 0f, 0f);
        SetCurve(clip, times, "localEulerAnglesRaw.y", 0f, 0f, 0f, 0f, 0f);
        SetCurve(clip, times, "localEulerAnglesRaw.z", 0f, 5f, 4f, -2f, 0f);
        SetCurve(clip, times, "m_LocalScale.x", 1f, 0.96f, 0.94f, 1.02f, 1f);
        SetCurve(clip, times, "m_LocalScale.y", 1f, 1.04f, 1.06f, 0.98f, 1f);
        SetCurve(clip, times, "m_LocalScale.z", 1f, 1f, 1f, 1f, 1f);

        clip.frameRate = 60f;
        clip.wrapMode = WrapMode.Once;
        EditorUtility.SetDirty(clip);
    }

    private static void ApplyDefaultHealCurves(AnimationClip clip)
    {
        float[] times = { 0f, 40f / 60f, 84f / 60f, 100f / 60f, 2f };

        SetCurve(clip, times, "m_AnchoredPosition.x", 20f, 1040f, 1040f, 520f, 20f);
        SetCurve(clip, times, "m_AnchoredPosition.y", -20f, 0f, 0f, -10f, -20f);
        SetCurve(clip, times, "localEulerAnglesRaw.x", 0f, 0f, 0f, 0f, 0f);
        SetCurve(clip, times, "localEulerAnglesRaw.y", 0f, 0f, 0f, 0f, 0f);
        SetCurve(clip, times, "localEulerAnglesRaw.z", 0f, -2f, -2f, 1f, 0f);
        SetCurve(clip, times, "m_LocalScale.x", 1f, 1.01f, 1.01f, 1f, 1f);
        SetCurve(clip, times, "m_LocalScale.y", 1f, 0.99f, 0.99f, 1f, 1f);
        SetCurve(clip, times, "m_LocalScale.z", 1f, 1f, 1f, 1f, 1f);

        clip.frameRate = 60f;
        clip.wrapMode = WrapMode.Once;
        EditorUtility.SetDirty(clip);
    }

    private static GameObject GetOrCreateHealParticlePrefab(string prefabPath, string objectName, bool isAura)
    {
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null) return existingPrefab;

        GameObject effectObject = CreateHealParticleObject(objectName, isAura);
        try
        {
            return PrefabUtility.SaveAsPrefabAsset(effectObject, prefabPath);
        }
        finally
        {
            Object.DestroyImmediate(effectObject);
        }
    }

    private static GameObject GetOrCreateAttackImpactParticlePrefab()
    {
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AttackImpactPrefabPath);
        if (existingPrefab != null) return existingPrefab;

        GameObject effectObject = CreateAttackImpactParticleObject();
        try
        {
            return PrefabUtility.SaveAsPrefabAsset(effectObject, AttackImpactPrefabPath);
        }
        finally
        {
            Object.DestroyImmediate(effectObject);
        }
    }

    private static ParticleSystem EnsureParticleInstance(Transform portraitHolder, GameObject particlePrefab)
    {
        Transform existing = portraitHolder.Find(particlePrefab.name);
        if (existing != null)
        {
            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(existing.gameObject);
            string sourcePath = AssetDatabase.GetAssetPath(source);
            if (sourcePath == AssetDatabase.GetAssetPath(particlePrefab))
            {
                existing.localScale = Vector3.one;
                return existing.GetComponent<ParticleSystem>();
            }

            Object.DestroyImmediate(existing.gameObject);
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(particlePrefab, portraitHolder);
        instance.name = particlePrefab.name;
        instance.transform.localScale = Vector3.one;
        return instance.GetComponent<ParticleSystem>();
    }

    private static GameObject CreateAttackImpactParticleObject()
    {
        GameObject effectObject = new GameObject(
            "Attack Impact Particles",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(ParticleSystem),
            typeof(UIParticle));

        effectObject.layer = LayerMask.NameToLayer("UI");
        RectTransform rectTransform = effectObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(520f, 520f);

        UIParticle uiParticle = effectObject.GetComponent<UIParticle>();
        uiParticle.scale = 50f;
        uiParticle.raycastTarget = false;

        ParticleSystem particles = effectObject.GetComponent<ParticleSystem>();
        ConfigureAttackImpactParticles(particles);

        ParticleSystemRenderer renderer = effectObject.GetComponent<ParticleSystemRenderer>();
        string materialPath = AssetDatabase.GUIDToAssetPath("9944483a3e009401ba5dcc42f14d5c63");
        renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        renderer.enabled = false;

        uiParticle.RefreshParticles();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        return effectObject;
    }

    private static void ConfigureAttackImpactParticles(ParticleSystem particles)
    {
        ParticleSystem.MainModule main = particles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 0.35f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        main.maxParticles = 40;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.42f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 6.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.34f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startColor = Color.white;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 22)
        });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.28f;
        shape.radiusThickness = 1f;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient fade = new Gradient();
        fade.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.9f, 0.35f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = fade;

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.35f),
            new Keyframe(0.15f, 1f),
            new Keyframe(1f, 0.1f)));

        ParticleSystem.NoiseModule noise = particles.noise;
        noise.enabled = true;
        noise.quality = ParticleSystemNoiseQuality.Low;
        noise.strength = 0.18f;
        noise.frequency = 1.2f;
        noise.scrollSpeed = 0.4f;
        noise.damping = true;
    }

    private static void RemoveChildIfPresent(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child != null) Object.DestroyImmediate(child.gameObject);
    }

    private static GameObject CreateHealParticleObject(string objectName, bool isAura)
    {
        GameObject effectObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(ParticleSystem),
            typeof(UIParticle));

        effectObject.layer = LayerMask.NameToLayer("UI");
        RectTransform rectTransform = effectObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = isAura ? Vector2.zero : new Vector2(120f, 0f);
        rectTransform.sizeDelta = isAura ? new Vector2(420f, 420f) : new Vector2(1200f, 420f);

        UIParticle uiParticle = effectObject.GetComponent<UIParticle>();
        uiParticle.scale = 50f;
        uiParticle.raycastTarget = false;

        ParticleSystem particles = effectObject.GetComponent<ParticleSystem>();
        ConfigureHealParticles(particles, isAura);

        ParticleSystemRenderer renderer = effectObject.GetComponent<ParticleSystemRenderer>();
        string materialPath = AssetDatabase.GUIDToAssetPath("9944483a3e009401ba5dcc42f14d5c63");
        renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        renderer.enabled = false;

        uiParticle.RefreshParticles();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        return effectObject;
    }

    private static void ConfigureHealParticles(ParticleSystem particles, bool isAura)
    {
        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.playOnAwake = false;
        main.duration = 2f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        main.maxParticles = isAura ? 100 : 140;
        main.startLifetime = isAura
            ? new ParticleSystem.MinMaxCurve(0.65f, 1.05f)
            : new ParticleSystem.MinMaxCurve(0.2f, 0.32f);
        main.startSpeed = isAura
            ? new ParticleSystem.MinMaxCurve(0.7f, 1.5f)
            : new ParticleSystem.MinMaxCurve(5f, 10f);
        main.startSize = isAura
            ? new ParticleSystem.MinMaxCurve(0.18f, 0.38f)
            : new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.3f, 1f, 0.65f, 0.85f),
            new Color(1f, 0.9f, 0.28f, 0.9f));

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.enabled = true;
        emission.rateOverTime = isAura ? 25f : 36f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = true;
        if (isAura)
        {
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 2.6f;
            shape.radiusThickness = 1f;
        }
        else
        {
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 7f;
            shape.radius = 0.22f;
            shape.rotation = new Vector3(0f, 90f, 0f);
        }

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient colorFade = new Gradient();
        colorFade.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.12f),
                new GradientAlphaKey(0.85f, 0.72f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = colorFade;

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.25f),
            new Keyframe(0.18f, 1f),
            new Keyframe(1f, 0.15f)));

        ParticleSystem.NoiseModule noise = particles.noise;
        noise.enabled = true;
        noise.quality = ParticleSystemNoiseQuality.Low;
        noise.strength = isAura ? 0.42f : 0.2f;
        noise.frequency = isAura ? 0.55f : 0.8f;
        noise.scrollSpeed = 0.25f;
        noise.damping = true;

        if (isAura)
        {
            ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.35f, 0.35f);
            velocity.y = new ParticleSystem.MinMaxCurve(0.8f, 1.7f);
            // Unity requires X, Y, and Z velocity curves to use the same mode.
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
        }
    }

    private static void SetCurve(AnimationClip clip, float[] times, string propertyName, params float[] values)
    {
        Keyframe[] keys = new Keyframe[times.Length];
        for (int i = 0; i < keys.Length; i++) keys[i] = new Keyframe(times[i], values[i]);

        AnimationCurve curve = new AnimationCurve(keys);
        for (int i = 0; i < keys.Length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
        }

        EditorCurveBinding binding = EditorCurveBinding.FloatCurve(
            "Portrait Holder",
            typeof(RectTransform),
            propertyName);

        AnimationUtility.SetEditorCurve(clip, binding, curve);
    }
}
#endif
