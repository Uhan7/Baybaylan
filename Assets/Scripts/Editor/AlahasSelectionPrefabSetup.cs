using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class AlahasSelectionPrefabSetup
{
    private const string InventoryPrefabPath =
        "Assets/Prefabs/Game HUD/Alahas Inventory Container.prefab";
    private const string ButtonPrefabPath =
        "Assets/Prefabs/General Reusables/Button.prefab";
    private const string LockedSpritePath =
        "Assets/Visuals/UI/iconsvg.png";
    private const string TooltipPrefabPath =
        "Assets/Prefabs/Game HUD/Alahas Info Popup.prefab";
    private const string AlahasSlotPrefabPath =
        "Assets/Prefabs/Alahas/Alahas Box.prefab";
    private const string TileAnimatorPath =
        "Assets/Animations/Tile Base.controller";
    private const string TileSetPrefabPath =
        "Assets/Prefabs/Game HUD/Tile Set.prefab";
    private const string MainGamePrefabPath =
        "Assets/Prefabs/!Core/Main Game.prefab";
    private const string SceneControllerPrefabPath =
        "Assets/Prefabs/Scene Transitions/Scene Controller.prefab";
    private const string AlahasFolder =
        "Assets/ScriptableObj Assets/Alahas";

    [MenuItem("Baybaylan/Setup Alahas Selection")]
    public static void Configure()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(InventoryPrefabPath);

        try
        {
            Transform grid = root.transform.Find("Background and Grid");
            if (grid == null)
                throw new InvalidOperationException("The inventory prefab has no 'Background and Grid' child.");

            AlahasSelectionController controller = root.GetComponent<AlahasSelectionController>();
            if (controller == null) controller = root.AddComponent<AlahasSelectionController>();

            Canvas foregroundCanvas = root.GetComponent<Canvas>();
            if (foregroundCanvas == null) foregroundCanvas = root.AddComponent<Canvas>();
            SerializedObject serializedCanvas = new SerializedObject(foregroundCanvas);
            serializedCanvas.FindProperty("m_OverrideSorting").boolValue = true;
            serializedCanvas.FindProperty("m_SortingOrder").intValue = 1000;
            serializedCanvas.ApplyModifiedPropertiesWithoutUndo();
            if (root.GetComponent<GraphicRaycaster>() == null)
                root.AddComponent<GraphicRaycaster>();

            Image gridBackdrop = grid.GetComponent<Image>();
            if (gridBackdrop != null)
            {
                gridBackdrop.color = new Color(0.035f, 0.055f, 0.075f, 0.68f);
                gridBackdrop.raycastTarget = true;
            }

            RuntimeAnimatorController tileAnimator =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(TileAnimatorPath);
            GameObject tooltipPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TooltipPrefabPath);

            foreach (Transform cell in grid)
            {
                if (cell.GetComponent<Image>() == null) continue;

                if (cell.GetComponent<AlahasInventoryItem>() == null)
                    cell.gameObject.AddComponent<AlahasInventoryItem>();
                if (cell.GetComponent<CanvasGroup>() == null)
                    cell.gameObject.AddComponent<CanvasGroup>();

                Animator animator = cell.GetComponent<Animator>();
                if (animator == null) animator = cell.gameObject.AddComponent<Animator>();
                animator.runtimeAnimatorController = tileAnimator;

                AlahasInfoPopup tooltip = cell.GetComponent<AlahasInfoPopup>();
                if (tooltip == null) tooltip = cell.gameObject.AddComponent<AlahasInfoPopup>();

                SerializedObject serializedTooltip = new SerializedObject(tooltip);
                serializedTooltip.FindProperty("tooltipObj").objectReferenceValue = tooltipPrefab;
                serializedTooltip.FindProperty("ToolTipDelay").floatValue = 0.2f;
                serializedTooltip.FindProperty("followMouse").boolValue = true;
                serializedTooltip.FindProperty("ToolTipPositionOffset").vector2Value =
                    new Vector2(-300f, -90f);
                serializedTooltip.ApplyModifiedPropertiesWithoutUndo();
            }

            Button proceedButton = GetOrCreateProceedButton(root);
            GameObject fallbackSlotPanel = GetOrCreateFallbackSlots(root);
            Sprite lockedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(LockedSpritePath);
            Alahas[] allAlahas = AssetDatabase.FindAssets("t:Alahas", new[] { AlahasFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<Alahas>)
                .Where(alahas => alahas != null)
                .OrderBy(alahas => alahas.alahasName)
                .ToArray();

            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("inventoryGrid").objectReferenceValue = grid;
            serializedController.FindProperty("fallbackSlotPanel").objectReferenceValue =
                fallbackSlotPanel;
            serializedController.FindProperty("lockedSprite").objectReferenceValue = lockedSprite;
            serializedController.FindProperty("proceedButton").objectReferenceValue = proceedButton;
            serializedController.FindProperty("slidingRoot").objectReferenceValue =
                root.GetComponent<RectTransform>();
            serializedController.FindProperty("foregroundSortingOrder").intValue = 1000;
            serializedController.FindProperty("slideDistance").floatValue = 900f;
            serializedController.FindProperty("slideDuration").floatValue = 0.45f;

            SerializedProperty alahasProperty = serializedController.FindProperty("allAlahas");
            alahasProperty.arraySize = allAlahas.Length;
            for (int i = 0; i < allAlahas.Length; i++)
                alahasProperty.GetArrayElementAtIndex(i).objectReferenceValue = allAlahas[i];

            serializedController.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, InventoryPrefabPath);
            ConfigureTileSetFallback();
            ConfigureSceneControllerFallback();
            AssetDatabase.SaveAssets();
            Debug.Log(
                $"Alahas selection configured with {allAlahas.Length} Alahas and " +
                $"{grid.childCount} inventory cells.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void ConfigureTileSetFallback()
    {
        GameObject selectorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(InventoryPrefabPath);
        AlahasSelectionController selector =
            selectorPrefab != null ? selectorPrefab.GetComponent<AlahasSelectionController>() : null;
        if (selector == null)
            throw new InvalidOperationException("The Alahas selector prefab has no controller.");

        GameObject tileSetRoot = PrefabUtility.LoadPrefabContents(TileSetPrefabPath);
        try
        {
            TileSet tileSet = tileSetRoot.GetComponent<TileSet>();
            if (tileSet == null)
                throw new InvalidOperationException("The Tile Set prefab has no TileSet component.");

            SerializedObject serializedTileSet = new SerializedObject(tileSet);
            serializedTileSet.FindProperty("alahasSelectionPrefab").objectReferenceValue = selector;
            serializedTileSet.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(tileSetRoot, TileSetPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(tileSetRoot);
        }
    }

    private static void ConfigureSceneControllerFallback()
    {
        GameObject selectorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(InventoryPrefabPath);
        AlahasSelectionController selector =
            selectorPrefab != null ? selectorPrefab.GetComponent<AlahasSelectionController>() : null;
        if (selector == null)
            throw new InvalidOperationException("The Alahas selector prefab has no controller.");

        GameObject sceneControllerRoot = PrefabUtility.LoadPrefabContents(SceneControllerPrefabPath);
        try
        {
            SceneController baseSceneController = sceneControllerRoot.GetComponent<SceneController>();
            if (baseSceneController == null)
                throw new InvalidOperationException("The Scene Controller prefab has no SceneController.");

            SerializedObject serializedBaseController = new SerializedObject(baseSceneController);
            serializedBaseController.FindProperty("alahasSelectionPrefab").objectReferenceValue = selector;
            serializedBaseController.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(sceneControllerRoot, SceneControllerPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(sceneControllerRoot);
        }

        GameObject mainGameRoot = PrefabUtility.LoadPrefabContents(MainGamePrefabPath);
        try
        {
            SceneController[] sceneControllers =
                mainGameRoot.GetComponentsInChildren<SceneController>(true);
            if (sceneControllers.Length == 0)
                throw new InvalidOperationException("The Main Game prefab has no SceneController.");

            foreach (SceneController sceneController in sceneControllers)
            {
                SerializedObject serializedController = new SerializedObject(sceneController);
                serializedController.FindProperty("alahasSelectionPrefab").objectReferenceValue = selector;
                serializedController.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(mainGameRoot, MainGamePrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(mainGameRoot);
        }
    }

    [MenuItem("Baybaylan/Validate Alahas Selection")]
    public static void Validate()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(InventoryPrefabPath);

        try
        {
            AlahasSelectionController controller = root.GetComponent<AlahasSelectionController>();
            Transform grid = root.transform.Find("Background and Grid");
            Button proceedButton = root.GetComponentsInChildren<Button>(true)
                .FirstOrDefault(button => button.name == "Proceed Button");
            Canvas foregroundCanvas = root.GetComponent<Canvas>();
            GraphicRaycaster raycaster = root.GetComponent<GraphicRaycaster>();

            if (controller == null) throw new InvalidOperationException("Selection controller is missing.");
            if (grid == null || grid.childCount == 0)
                throw new InvalidOperationException("Inventory cells are missing.");
            if (proceedButton == null) throw new InvalidOperationException("Proceed button is missing.");
            if (foregroundCanvas == null || foregroundCanvas.sortingOrder < 1000)
                throw new InvalidOperationException("Foreground canvas is not configured.");
            if (raycaster == null)
                throw new InvalidOperationException("Selector GraphicRaycaster is missing.");
            if (grid.Cast<Transform>().Any(cell =>
                cell.GetComponent<Image>() != null &&
                (cell.GetComponent<AlahasInventoryItem>() == null ||
                 cell.GetComponent<CanvasGroup>() == null ||
                 cell.GetComponent<Animator>()?.runtimeAnimatorController == null ||
                 cell.GetComponent<AlahasInfoPopup>() == null)))
                throw new InvalidOperationException(
                    "An inventory cell is missing drag, animation, or tooltip support.");

            Debug.Log(
                $"Alahas selection validation passed: {grid.childCount} cells and a Proceed button.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static Button GetOrCreateProceedButton(GameObject root)
    {
        Button existing = root.GetComponentsInChildren<Button>(true)
            .FirstOrDefault(button => button.name == "Proceed Button");
        GameObject buttonObject;

        if (existing != null)
        {
            buttonObject = existing.gameObject;
        }
        else
        {
            GameObject buttonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ButtonPrefabPath);
            if (buttonPrefab == null)
                throw new InvalidOperationException($"Could not load {ButtonPrefabPath}.");

            buttonObject = PrefabUtility.InstantiatePrefab(buttonPrefab, root.transform) as GameObject;
            buttonObject.name = "Proceed Button";
        }

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(500f, -440f);
        rect.sizeDelta = new Vector2(540f, 110f);
        rect.localScale = Vector3.one;

        TMP_Text label = buttonObject.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.text = "MAGPATULOY";
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.enableAutoSizing = true;
            label.fontSizeMin = 22f;
            label.fontSizeMax = 40f;
            label.raycastTarget = false;
        }

        Image buttonImage = buttonObject.GetComponent<Image>();
        if (buttonImage != null) buttonImage.raycastTarget = true;

        return buttonObject.GetComponent<Button>();
    }

    private static GameObject GetOrCreateFallbackSlots(GameObject root)
    {
        Transform existing = root.transform.Find("Fallback Selection Slots");
        GameObject panel = existing != null
            ? existing.gameObject
            : new GameObject(
                "Fallback Selection Slots",
                typeof(RectTransform),
                typeof(GridLayoutGroup));

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.SetParent(root.transform, false);
        panelRect.anchorMin = new Vector2(0.14f, 0.5f);
        panelRect.anchorMax = new Vector2(0.14f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(380f, 250f);
        panelRect.localScale = Vector3.one;

        GridLayoutGroup layout = panel.GetComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(105f, 105f);
        layout.spacing = new Vector2(20f, 20f);
        layout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        layout.startAxis = GridLayoutGroup.Axis.Horizontal;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 3;

        GameObject slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AlahasSlotPrefabPath);
        if (slotPrefab == null)
            throw new InvalidOperationException($"Could not load {AlahasSlotPrefabPath}.");

        for (int i = panel.transform.childCount; i < 6; i++)
        {
            GameObject slot = PrefabUtility.InstantiatePrefab(slotPrefab, panel.transform) as GameObject;
            slot.name = $"Selection Slot {i + 1:00}";
        }

        panel.SetActive(false);
        return panel;
    }
}
