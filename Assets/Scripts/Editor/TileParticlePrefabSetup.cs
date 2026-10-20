#if UNITY_EDITOR
using Coffee.UIExtensions;
using UnityEditor;
using UnityEngine;

public static class TileParticlePrefabSetup
{
    private const string ParticleFolder = "Assets/Prefabs/VFX/Tile Particles";
    private const string TileBasePath = "Assets/Prefabs/Titik Tiles/Tile Base.prefab";
    private const string DragPath = ParticleFolder + "/Tile Drag Particles.prefab";
    private const string DropPath = ParticleFolder + "/Tile Drop Particles.prefab";
    private const string ClickPath = ParticleFolder + "/Tile Click Particles.prefab";
    private const string ScorePath = ParticleFolder + "/Tile Score Particles.prefab";
    private const string ParticleMaterialGuid = "9944483a3e009401ba5dcc42f14d5c63";

    private enum BurstStyle
    {
        Drag,
        Drop,
        Score
    }

    [MenuItem("Tools/Baybaylan/Create Default Tile Particle Prefabs")]
    public static void CreateDefaultTileParticlePrefabs()
    {
        EnsureFolder("Assets/Prefabs/VFX", "Tile Particles");

        GameObject dragPrefab = CreateParticlePrefab(DragPath, "Tile Drag Particles", BurstStyle.Drag);
        GameObject dropPrefab = CreateParticlePrefab(DropPath, "Tile Drop Particles", BurstStyle.Drop);
        GameObject clickPrefab = CreateParticlePrefab(ClickPath, "Tile Click Particles", BurstStyle.Drop);
        GameObject scorePrefab = CreateParticlePrefab(ScorePath, "Tile Score Particles", BurstStyle.Score);

        ConnectParticlesToTileBase(dragPrefab, dropPrefab, clickPrefab, scorePrefab);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Created editable tile particle prefabs in {ParticleFolder} and connected them to Tile Base.");
    }

    [MenuItem("Tools/Baybaylan/Create Tile Click Particle From Drop")]
    public static void CreateTileClickParticleFromDrop()
    {
        EnsureFolder("Assets/Prefabs/VFX", "Tile Particles");

        if (AssetDatabase.LoadAssetAtPath<GameObject>(DropPath) == null)
            throw new MissingReferenceException($"Missing drop particle prefab at {DropPath}.");

        if (AssetDatabase.LoadAssetAtPath<GameObject>(ClickPath) == null
            && !AssetDatabase.CopyAsset(DropPath, ClickPath))
        {
            throw new UnityException("Could not copy the drop particle prefab for tile clicks.");
        }

        GameObject clickRoot = PrefabUtility.LoadPrefabContents(ClickPath);
        try
        {
            clickRoot.name = "Tile Click Particles";
            PrefabUtility.SaveAsPrefabAsset(clickRoot, ClickPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(clickRoot);
        }

        GameObject clickPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ClickPath);
        ConnectClickParticleToTileBase(clickPrefab);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Copied DROP juice to Tile Click Particles and connected it to Tile Base.");
    }

    private static GameObject CreateParticlePrefab(string path, string objectName, BurstStyle style)
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
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(500f, 500f);

        UIParticle uiParticle = effectObject.GetComponent<UIParticle>();
        uiParticle.scale = 100f;

        ParticleSystem particles = effectObject.GetComponent<ParticleSystem>();
        ConfigureParticles(particles, style);

        ParticleSystemRenderer renderer = effectObject.GetComponent<ParticleSystemRenderer>();
        string materialPath = AssetDatabase.GUIDToAssetPath(ParticleMaterialGuid);
        renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.enabled = false;

        uiParticle.RefreshParticles();
        // UIParticle overrides Graphic.raycastTarget to always return false, so
        // these decorative effects cannot steal tile pointer events.
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(effectObject, path);
        Object.DestroyImmediate(effectObject);
        return prefab;
    }

    private static void ConfigureParticles(ParticleSystem particles, BurstStyle style)
    {
        ParticleSystem.MainModule main = particles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 0.55f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        main.maxParticles = style == BurstStyle.Score ? 48 : 32;
        main.startLifetime = GetLifetime(style);
        main.startSpeed = GetSpeed(style);
        main.startSize = GetSize(style);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startColor = GetColors(style);

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, (short)GetBurstCount(style))
        });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = style == BurstStyle.Score ? 0.42f : 0.34f;
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
                new GradientAlphaKey(0.9f, 0.45f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = fade;

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.35f),
            new Keyframe(0.16f, 1f),
            new Keyframe(1f, 0.08f)));

        ParticleSystem.NoiseModule noise = particles.noise;
        noise.enabled = true;
        noise.quality = ParticleSystemNoiseQuality.Low;
        noise.strength = style == BurstStyle.Score ? 0.28f : 0.18f;
        noise.frequency = 1.1f;
        noise.scrollSpeed = 0.35f;
        noise.damping = true;
    }

    private static ParticleSystem.MinMaxCurve GetLifetime(BurstStyle style)
    {
        return style switch
        {
            BurstStyle.Drag => new ParticleSystem.MinMaxCurve(0.16f, 0.3f),
            BurstStyle.Drop => new ParticleSystem.MinMaxCurve(0.22f, 0.4f),
            _ => new ParticleSystem.MinMaxCurve(0.35f, 0.62f)
        };
    }

    private static ParticleSystem.MinMaxCurve GetSpeed(BurstStyle style)
    {
        return style switch
        {
            BurstStyle.Drag => new ParticleSystem.MinMaxCurve(0.8f, 2.1f),
            BurstStyle.Drop => new ParticleSystem.MinMaxCurve(1.8f, 3.8f),
            _ => new ParticleSystem.MinMaxCurve(2.5f, 5.8f)
        };
    }

    private static ParticleSystem.MinMaxCurve GetSize(BurstStyle style)
    {
        return style switch
        {
            BurstStyle.Drag => new ParticleSystem.MinMaxCurve(0.13f, 0.28f),
            BurstStyle.Drop => new ParticleSystem.MinMaxCurve(0.17f, 0.36f),
            _ => new ParticleSystem.MinMaxCurve(0.2f, 0.48f)
        };
    }

    private static ParticleSystem.MinMaxGradient GetColors(BurstStyle style)
    {
        return style switch
        {
            BurstStyle.Drag => new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.9f, 0.55f, 0.95f),
                new Color(0.9f, 0.65f, 0.25f, 0.9f)),
            BurstStyle.Drop => new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.97f, 0.82f, 1f),
                new Color(0.7f, 0.9f, 1f, 0.95f)),
            _ => new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.96f, 0.55f, 1f),
                new Color(1f, 0.52f, 0.16f, 1f))
        };
    }

    private static int GetBurstCount(BurstStyle style)
    {
        return style switch
        {
            BurstStyle.Drag => 12,
            BurstStyle.Drop => 20,
            _ => 32
        };
    }

    private static void ConnectParticlesToTileBase(
        GameObject dragPrefab,
        GameObject dropPrefab,
        GameObject clickPrefab,
        GameObject scorePrefab)
    {
        GameObject tileRoot = PrefabUtility.LoadPrefabContents(TileBasePath);
        try
        {
            Draggable draggable = tileRoot.GetComponent<Draggable>();
            if (draggable == null)
                throw new MissingComponentException("Tile Base needs a Draggable component.");

            RemoveChild(tileRoot.transform, "Tile Drag Particles");
            RemoveChild(tileRoot.transform, "Tile Drop Particles");
            RemoveChild(tileRoot.transform, "Tile Click Particles");
            RemoveChild(tileRoot.transform, "Tile Score Particles");

            ParticleSystem drag = AddNestedParticle(dragPrefab, tileRoot.transform);
            ParticleSystem drop = AddNestedParticle(dropPrefab, tileRoot.transform);
            ParticleSystem click = AddNestedParticle(clickPrefab, tileRoot.transform);
            ParticleSystem score = AddNestedParticle(scorePrefab, tileRoot.transform);

            SerializedObject serializedDraggable = new SerializedObject(draggable);
            serializedDraggable.FindProperty("dragParticles").objectReferenceValue = drag;
            serializedDraggable.FindProperty("dropParticles").objectReferenceValue = drop;
            serializedDraggable.FindProperty("clickParticles").objectReferenceValue = click;
            serializedDraggable.FindProperty("scoreParticles").objectReferenceValue = score;
            serializedDraggable.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(tileRoot, TileBasePath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(tileRoot);
        }
    }

    private static void ConnectClickParticleToTileBase(GameObject clickPrefab)
    {
        GameObject tileRoot = PrefabUtility.LoadPrefabContents(TileBasePath);
        try
        {
            Draggable draggable = tileRoot.GetComponent<Draggable>();
            if (draggable == null)
                throw new MissingComponentException("Tile Base needs a Draggable component.");

            RemoveChild(tileRoot.transform, "Tile Click Particles");
            ParticleSystem click = AddNestedParticle(clickPrefab, tileRoot.transform);

            SerializedObject serializedDraggable = new SerializedObject(draggable);
            serializedDraggable.FindProperty("clickParticles").objectReferenceValue = click;
            serializedDraggable.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(tileRoot, TileBasePath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(tileRoot);
        }
    }

    private static ParticleSystem AddNestedParticle(GameObject prefab, Transform parent)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        RectTransform rectTransform = instance.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localScale = Vector3.one;
        return instance.GetComponent<ParticleSystem>();
    }

    private static void RemoveChild(Transform parent, string childName)
    {
        Transform existing = parent.Find(childName);
        if (existing != null) Object.DestroyImmediate(existing.gameObject);
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
    }
}
#endif
