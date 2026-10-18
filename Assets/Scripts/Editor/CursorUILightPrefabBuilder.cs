#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CursorUILightPrefabBuilder
{
    private const string ShaderName = "UI/Baybaylan Cursor Light";
    private const string MaterialFolder = "Assets/Materials/UI";
    private const string MaterialPath = MaterialFolder + "/Cursor UI Light.mat";
    private const string PrefabPath = "Assets/Prefabs/VFX/Cursor UI Light.prefab";
    private const string MainGamePrefabPath = "Assets/Prefabs/!Core/Main Game.prefab";

    [MenuItem("Tools/Baybaylan/Create Cursor UI Light Prefab")]
    public static void CreatePrefab()
    {
        Shader shader = Shader.Find(ShaderName);
        if (shader == null)
        {
            throw new System.InvalidOperationException($"Could not find shader '{ShaderName}'.");
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null)
        {
            material = new Material(shader) { name = "Cursor UI Light" };
            AssetDatabase.CreateAsset(material, MaterialPath);
        }
        else
        {
            material.shader = shader;
        }

        material.SetColor("_DarknessColor", new Color(0f, 0f, 0f, 0.55f));
        material.SetVector("_LightCenter", new Vector4(0.5f, 0.5f, 0f, 0f));
        material.SetVector("_RectSize", new Vector4(1920f, 1080f, 0f, 0f));
        material.SetFloat("_InnerRadius", 10f);
        material.SetFloat("_OuterRadius", 2200f);
        EditorUtility.SetDirty(material);

        GameObject root = new GameObject(
            "Cursor UI Light",
            typeof(CursorUILight));

        GameObject canvasObject = new GameObject(
            "Overlay Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler));

        int uiLayer = LayerMask.NameToLayer("UI");
        root.layer = uiLayer >= 0 ? uiLayer : 0;
        root.transform.localScale = Vector3.one;
        canvasObject.layer = root.layer;
        canvasObject.transform.SetParent(root.transform, false);
        canvasObject.transform.localScale = Vector3.one;

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject overlayObject = new GameObject(
            "Darkness Overlay",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        overlayObject.layer = root.layer;
        overlayObject.transform.SetParent(canvasObject.transform, false);

        RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = overlayObject.GetComponent<Image>();
        overlayImage.color = Color.white;
        overlayImage.material = material;
        overlayImage.raycastTarget = false;

        SerializedObject lightSettings = new SerializedObject(root.GetComponent<CursorUILight>());
        lightSettings.FindProperty("overlayImage").objectReferenceValue = overlayImage;
        lightSettings.ApplyModifiedPropertiesWithoutUndo();

        root.transform.localScale = Vector3.one;
        canvasObject.transform.localScale = Vector3.one;
        GameObject cursorLightPrefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        InstallInMainGame(cursorLightPrefab);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Created editable cursor UI light prefab at {PrefabPath}.");
    }

    [MenuItem("Tools/Baybaylan/Validate Cursor UI Light Prefab")]
    public static void ValidatePrefab()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            throw new System.InvalidOperationException($"Missing prefab at {PrefabPath}.");
        }

        CursorUILight light = prefab.GetComponent<CursorUILight>();
        Canvas canvas = prefab.GetComponentInChildren<Canvas>(true);
        Image overlay = prefab.GetComponentInChildren<Image>(true);

        if (light == null || canvas == null || overlay == null || overlay.material == null)
        {
            throw new System.InvalidOperationException("Cursor UI Light prefab is missing a required component or material.");
        }

        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay || overlay.raycastTarget)
        {
            throw new System.InvalidOperationException("Cursor UI Light must be an overlay canvas and must not block pointer input.");
        }

        if (overlay.material.shader == null || overlay.material.shader.name != ShaderName)
        {
            throw new System.InvalidOperationException($"Cursor UI Light is not using '{ShaderName}'.");
        }

        GameObject mainGame = AssetDatabase.LoadAssetAtPath<GameObject>(MainGamePrefabPath);
        if (mainGame == null || mainGame.GetComponentInChildren<CursorUILight>(true) == null)
        {
            throw new System.InvalidOperationException("Main Game prefab does not contain the Cursor UI Light prefab.");
        }

        Debug.Log("Cursor UI Light prefab validation passed.");
    }

    private static void InstallInMainGame(GameObject cursorLightPrefab)
    {
        GameObject mainGameRoot = PrefabUtility.LoadPrefabContents(MainGamePrefabPath);
        try
        {
            if (mainGameRoot.GetComponentInChildren<CursorUILight>(true) != null)
            {
                return;
            }

            GameObject lightInstance = (GameObject)PrefabUtility.InstantiatePrefab(
                cursorLightPrefab,
                mainGameRoot.scene);
            lightInstance.name = cursorLightPrefab.name;
            lightInstance.transform.SetParent(mainGameRoot.transform, false);
            PrefabUtility.SaveAsPrefabAsset(mainGameRoot, MainGamePrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(mainGameRoot);
        }
    }
}
#endif
